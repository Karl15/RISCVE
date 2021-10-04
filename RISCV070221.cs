
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.CodeAnalysis.Syntax;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Windows.Forms;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.CodeAnalysis.Syntax;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RVNS;
{
    public class RISCV
    {
        public MEMORY Mem = new MEMORY();
        public uint[] Regs = new uint[32], Idcd = new uint[512];
        int Iar = 0, Mar = 0, Romar = 0;
        uint IR = 0;
        uint Imm20, Imm12, PC;
        uint OpCode;
        string type;
        uint fun3, fun7;
        int rd, rs1, rs2;
        int sAdr, bAdr;
        bool baseInst, immNeg;
        int[] ImmHi = new int[64];
        int[] ImmLo = new int[64];
        int r8, r9;
        //Alu alu = new Alu();
        StringBuilder sB;




        uint[] nMsk = new uint[33]  {0x0000, 0x0001, 0x0003, 0x0007, 0x000f,
                                               0x001f, 0x003f, 0x007f, 0x00ff,
                                               0x01ff, 0x03ff, 0x07ff, 0x0fff,
                                               0x1fff, 0x3fff, 0x7fff, 0xffff,
                                               0x0001ffff, 0x0003ffff, 0x0007ffff, 0x000fffff,
                                               0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
                                               0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff,
                                               0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};

        public void Build(ref RISCV rISCV, string fPath, ListBox lB1in, Parser pIn)
        {
            rISCV.Mem.iList.Add(0);
            Romar = 0;
            Idcd[Romar] = 0;
            //opL = new XVal(node, 0);
            ListBox lB1 = lB1in;
            //int lineno = 0;
            StreamReader sR = new StreamReader(@"C:\Users\Karl\Dropbox\ASTEngine\ASTDemo.cs");
            StringBuilder sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
            sB = new StringBuilder("main() { y = 2 + 3 + 4 - 5; x = y - 6; }");
            //sB = new StringBuilder("y = 2 + 3 + 4 - 5; x = y - 6;");
            CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
            SyntaxNode node = tree.GetRoot();
            CEngineWalker cEwalker = new CEngineWalker();
            cEwalker.parser = pIn;
            cEwalker.Visit(node);
            Console.WriteLine(tree);
            return;
            //decode();
        }

        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        // cwCt = 0	          mem[cwCt.qa]                            mem[cwCt.qa]
        //					                      mem[cwCt.qa + 1]                       mem[cwCt.qa + 1]           
        //										  va[sCw.qb]          vA[op1@, stkCt.qa]
        //										  vb[oCw.qb]          vB[oCw.qb]
        // cwCtA = 2		  cwCtA = nxtScw                                                                    cwCtA = nxtScw
        //                    va,vb addr          op1, op2, ocw       TOS, op2, ocw

        public void decode(ref RISCV rISCV)
        {
            sB = new StringBuilder();
            rISCV.Mem.iList.Add(0);
            IR = Mem.iList[0];
            OpCode = IR & 0x7f; // OpCode = IR low 7 bits
            IR >>= 7;   // shift OpCode out
            rd = (int)IR & 0x1f;    // rd is low order 5 bits after shift
            IR >>= 5;   // shift out rd
            Imm20 = IR & 0xfffff;   // Imm20 is low 20 bits after shift
            fun3 = IR & 0x7;        // fun 3 is low 3 bits after shift
            IR >>= 3;
            rs1 = (int)IR & 0x1f;   // rs1 is low 5 bits after shift
            IR >>= 5;
            Imm12 = IR;             //Imm12 is low 12 bits after shift
            rs2 = (int)IR & 0x1f;   //rs2 is low 5 bits after shift
            IR >>= 5;
            fun7 = IR & 0x7f;       //fun7 is low 7 bits after shift
            switch (OpCode)
            {
                case 0x23:
                    type = "Stype";
                    //rd = (int)(IR & 0x1f);
                    //Imm12 = IR & 0x0000001f;
                    IR >>= 5;
                    switch (IR & 0x00000007)
                    {
                        case 0x0:    // sb
                            break;
                        case 0x1:    // sh
                            break;
                        case 0x2:    // sw
                            break;
                    }
                    break;
                case 0x37:
                    type = "Utype";     // lui
                    rd = (int)(IR & 0x1f);
                    Regs[rd] = IR & 0x000fffff;
                    break;
                case 0x17:
                    type = "Utype";     // auipc
                    IR >>= 5;
                    Regs[rd] = PC + (IR & 0x000fffff);
                    break;
                case 0x6f:
                    type = "Jtype";     // jal
                    IR >>= 5;
                    rd = (int)(IR & 0x1f);
                    Regs[rd] = PC + 4;
                    PC += (IR & 0xfffff000);
                    break;
                case 0x67:
                    type = "Jtype";     // jalr
                    rd = (int)(IR & 0x1f);
                    IR >>= 5;
                    Regs[rd] = PC + 4;
                    //PC += (Regs[rs1] & 0xfff00000);   // fix this
                    break;
                case 0x63:
                    type = "Btype";
                    fun3 = (uint)(IR & 0x7);
                    IR >>= 3;
                    switch (fun3)
                    {
                        case 0x0:    // beq
                            break;
                        case 0x1:    // bne
                            break;
                        case 0x4:    // blt
                            break;
                        case 0x5:    // bge
                            break;
                        case 0x6:    // bltu
                            break;
                        case 0x7:    // bgeu
                            break;
                    }

                    break;
                case 0x03:
                    type = "Itype";
                    rd = (int)(IR & 0x1f);
                    Imm12 = IR & 0xfff;
                    fun3 = (uint)(IR & 0x7);
                    IR >>= 3;
                    switch (fun3)
                    {
                        case 0x0:    // lb
                            break;
                        case 0x1:    // lh
                            break;
                        case 0x2:    // lw
                            break;
                        case 0x4:    // lbu
                            break;
                        case 0x5:    // lhu
                            break;
                    }
                    break;
                case 0x13:
                    type = "Itype";
                    rd = (int)(IR & 0x1f);
                    fun3 = IR & 0x7;
                    IR >>= 3;
                    switch (fun3)
                    {
                        case 0x0000:    // addi
                            Regs[rd] = Regs[rs1] += Imm12;
                            break;
                        case 0x7000:    // andi
                            Regs[rd] = Regs[rs1] &= Imm12;
                            break;
                        case 0x6000:    // ori
                            Regs[rd] = Regs[rs1] |= Imm12;
                            break;
                        case 0x4000:    // xori
                            Regs[rd] = Regs[rs1] ^= Imm12;
                            break;
                        case 0x2000:    // slti
                            break;
                        case 0x3000:    // sltiu
                            break;
                    }
                    break;
                case 0x33:
                    type = "Rtype";
                    IR >>= 7;
                    switch (IR & 0x7)
                    {
                        case 0x0:    // add/sub
                            Regs[rd] = (IR & 0x40000000) == 0 ? Regs[rs1] + Regs[rs2] : Regs[rs1] - Regs[rs2];
                            break;
                        case 0x7:    // and
                            Regs[rd] = Regs[rs1] &= Regs[rs2];
                            break;
                        case 0x6:    // or
                            Regs[rd] = Regs[rs1] |= Regs[rs2];
                            break;
                        case 0x4:    // xor
                            Regs[rd] = Regs[rs1] ^= Regs[rs2];
                            break;
                        case 0x1:    // sll
                            break;
                        case 0x2:    // slt
                            break;
                        case 0x3:    // sltu
                            break;
                        case 0x5:    // srl/sra
                            break;
                    }
                    break;
            }
        }

        public enum RVenum
        {
            LUI = 0x00000037,
            AUIPC = 0X00000017,
            JAL = 0X0000006F,
            JALR = 0X00000067,
            BEQ = 0x00000063,
            BNE = 0x00000063,
            BLT = 0x00000063,
            BGE = 0x00000063,
            BLTU = 0x00000063,
            BGEU = 0x00000063,
            LB = 0X00000003,
            LH = 0X00001003,
            LW = 0X00002003,
            LBU = 0X00004003,
            LHU = 0X00005003,
            SB = 0X00000023,
            SH = 0X00000023,
            SW = 0X00000023,
            ADDI = 0X00000013,
            SLTI = 0X00002013,
            SLTIU = 0X00003013,
            XORI = 0X00004013,
            ORI = 0X00006013,
            ANDI = 0X00007013,
            SLLI = 0x00001013,
            SRLI = 0X00003033,
            SRAI = 0X40003033,
            ADD = 0x00000033,
            SUB = 0x40000033,
            SLL = 0X00001033,
            SLT = 0X00002033,
            SLTU = 0X00003033,
            XOR = 0x00004033,
            SRL = 0X00005033,
            SRAL = 0X40005033,
            OR = 0x00006033,
            AND = 0x00007033,
        }


        public enum FUNCodes
        {
            LUI = 0x37, AUIPC = 0X17, JAL = 0X6F, JALR = 0X67,
            RType = 0x33, IType = 0x13, SType = 0x13, BType = 0x63,
            BEQ = 0X0000, BNE = 0X01000, BLT = 0X04000,
            BGE = 0X5000, BLTU = 0X6000, BGEU = 0X7000,
            LB = 0X0000, LH = 0X1000, LW = 0X2000,
            LBU = 0X4000, LHU = 0X5000,
            SB = 0X0000, SH = 0X1000, SW = 0X2000,
            ADDI = 0X0000, SLTI = 0X2000, SLTIU = 0X3000,
            XORI = 0X4000, ORI = 0X6000, ANDI = 0X7000,
            SLLI = 0X1000, SRLI = 0X5000, SRAI = 0X5000,
            ADD = 0X0000, SUB = 0X40000000, SLL = 0X1000,
            SLT = 0X2000, SLTU = 0X3000, XOR = 0X4000,
            SRL = 0X5000, SRA = 0X40005000, OR = 0X6000,
            AND = 0X7000,
            MUL = 0X02000000, MULH = 0X02001000, MULSHU = 0X02002000, MULHU = 0X02003000,
            DIV = 0X0204000, DIVU = 0X02005000, REM = 0X02006000, REMU = 0X02007000,
            FENCE = 0X0000000F, ECALL = 0X00000073, EBREAK = 0X00100073
        }

        ////[Flags]
        //public enum RVenum
        //{
        //    LUI = 0x00000037,
        //    AUIPC = 0X00000017,
        //    JAL = 0X0000006F,
        //    JALR = 0X00000067,
        //    BEQ = 0x00000063,
        //    BNE = 0x00001063,
        //    BLT = 0x00004063,
        //    BGE = 0x00005063,
        //    BLTU = 0x00006063,
        //    BGEU = 0x00007063,
        //    LB = 0X00000003,
        //    LH = 0X00001003,
        //    LW = 0X00002003,
        //    LBU = 0X00004003,
        //    LHU = 0X00005003,
        //    SB = 0X00000023,
        //    SH = 0X00000023,
        //    SW = 0X00000023,
        //    ADDI = 0X00000013,
        //    SLTI = 0X00002013,
        //    SLTIU = 0X00003013,
        //    XORI = 0X00004013,
        //    ORI = 0X00006013,
        //    ANDI = 0X00007013,
        //    SLLI = 0x00001013,
        //    SRLI = 0X00003033,
        //    SRAI = 0X40003033,
        //    ADD = 0x00000033,
        //    SUB = 0x40000033,
        //    SLL = 0X00001033,
        //    SLT = 0X00002033,
        //    SLTU = 0X00003033,
        //    XOR = 0x00004033,
        //    SRL = 0X00005033,
        //    SRAL = 0X40005033,
        //    OR = 0x00006033,
        //    AND = 0x00007033,
        //}


        public void LI()
        {

        }
        public void LD()
        {
            Mem.iList.Add(0x1003);
            uint opc = (uint)RVenum.ADD;
            if (Mem.iList[Mem.iList.Count - 1] != opc)
            { }
        }
        public void ST()
        {
            Mem.iList.Add(0x2003);
        }
        public void ADD()
        {
            Mem.iList.Add(0x0033);
        }
        public void SUB()
        {
            Mem.iList.Add(0x0033);
        }
        public void MUL()
        {
            Mem.iList.Add(0x020000033);
        }
        public void DIV()
        {
            Mem.iList.Add(0x020004033);
        }
        public void AND()
        {
            Mem.iList.Add(0x3B03);
        }
        public void OR()
        {
            Mem.iList.Add(0x2B03);
        }
        public void XOR()
        {
            Mem.iList.Add(0x2033);
        }
        uint mk_fld(string fld)    //  , out string[] ix
        {
            //ix = fld.Trim().Split(bkts, StringSplitOptions.RemoveEmptyEntries);
            int ix1 = 0, ix2 = 0;
            //xParse(ix[1], out ix1);
            //xParse(ix[2], out ix2);
            uint nM1;
            if (ix1 > ix2)
            {
                nM1 = nMsk[ix1 - ix2 + 1];
                return (nM1 <<= ix2);
            }
            else
            {
                nM1 = nMsk[ix2 - ix1 + 1];
                return (nM1 <<= ix1);
            }
        }
        class ALU
        {
            public uint aluOut(uint l, char oper, uint r)
            {
                switch (oper)
                {
                    case '+':
                        return l + r;
                    case '-':
                        return l - r;
                    case '&':
                        return l & r;
                    case '|':
                        return l | r;
                    case '^':
                        return l ^ r;
                    default:
                        MessageBox.Show("Invalid operator");
                        return 0xffffffff;
                }
            }

            public int add(int a, int b)
            {
                return a + b;
            }

            public int sub(int a, int b)
            {
                return a - b;
            }

            public int and(int a, int b)
            {
                return a & b;
            }

            public int or(int a, int b)
            {
                return a | b;
            }

            public int xor(int a, int b)
            {
                return a ^ b;
            }
            public int mpy(int a, int b)
            {
                int op1, op2, rslt;
                long acc = 0;
                if (a < 0)
                    op1 = ~a + 1;   // make pos
                if (b < 0)
                    op2 = ~b + 1;   // make pos
                                    //a = 0x4;
                                    //b = 0x2;
                rslt = a * b;
                while (b != 0)
                {
                    if ((b & 0x1) == 1)
                        acc += b;
                    b >>= 1;
                    if (b == 0)
                        return rslt;
                    //else
                    //    if(b == 0)
                    //        return a * b;
                }
                return -1;
            }

        }


        public class MEMORY
        {
            public List<uint> mem = new List<uint>();
            public List<uint> iList = new List<uint>();
            int mar;
            uint mdir, mdor;
            public uint read(int adr)
            {
                return mem[adr];
            }
            public void write(int adr, uint data)
            {
                mar = adr;
                mdir = data;
                mem[mar] = mdir;
            }
        }

        public class MMIO
        {
            public uint cmnd, stat, dataIn, dataOut;
        }
        public void Run()
        {
            IR = 0x06900003;
            if ((IR & 0x00000003) == 3)
                baseInst = true;
            if ((IR & 0x80000000) == 0x80000000)
                immNeg = true;
            IR = Mem.iList[Iar];
            if (baseInst)
            {
                switch (IR & 0x00000007F)
                {
                    case 0x00000003:
                        type = "I_Type";
                        switch (IR & 0x00000000)
                        {
                            case 0x00000000:
                                // load byte
                                break;
                            case 0x00010000:
                                // load hw
                                break;
                            case 0x00020000:
                                // load word
                                break;
                            case 0x00400000:
                                // load byte unsigned
                                break;
                            case 0x00500000:
                                // load hw unsigned
                                break;
                        }
                        // load byte
                        break;
                    case 0x00000023:
                        type = "S_Type";
                        switch (IR & 0x00000023)
                        {
                            case 0x00000023:
                                // store byte
                                break;
                            case 0x00010023:
                                // store hw
                                break;
                            case 0x00020023:
                                // store word
                                break;
                        }
                        break;
                    case 0x00000063:
                        type = "B_Type";
                        switch (IR & 0x00007000)
                        {
                            case 0x00000000:
                                if (Regs[rs1] == Regs[rs2])
                                    PC += Imm12 >> 20;
                                break;
                            case 0x00000001:
                                if (Regs[rs1] != Regs[rs2])
                                    PC += Imm12 >> 20;
                                break;
                            case 0x00000004:
                                if (Regs[rs1] < Regs[rs2])
                                    PC += Imm12 >> 20;
                                break;
                            case 0x00000005:
                                if (Regs[rs1] != Regs[rs2])
                                    PC += Imm12 >> 20;
                                break;
                            case 0x00000006:
                                if (Regs[rs1] < Regs[rs2])
                                    PC += Imm12 >> 20;
                                break;
                            case 0x00000007:
                                if (Regs[rs1] >= Regs[rs2])
                                    PC += Imm12 >> 20;
                                break;
                        }
                        break;
                    case 0x00000033:
                        type = "R_Type";
                        switch (IR & 0x00070033)
                        {
                            case 0x00000033:
                                // add/sub
                                break;
                            case 0x00010033:
                                // SLL
                                break;
                            case 0x00020033:
                                // SLT
                                break;
                            case 0x00030033:
                                // SLTU
                                break;
                            case 0x00040033:
                                // xor
                                break;
                            case 0x00050033:
                                // sra
                                break;
                            case 0x00060033:
                                // or
                                break;
                            case 0x00070033:
                                // and
                                break;
                        }
                        break;
                    case 0x00000037:
                        type = "U_Type";
                        Regs[rs1] = IR & 0xfffff000;
                        break;
                    case 0x0000006F:
                        type = "J_Type";
                        Regs[rd] = PC;
                        //IC = 
                        break;
                }

            }
        }


        public class CEngineWalker : CSharpSyntaxWalker
        {
            //NOTE: Make sure you invoke the base constructor with 
            //the correct SyntaxWalkerDepth. Otherwise VisitToken()
            //will never get run.
            //public CEngineWalker() : base(SyntaxWalkerDepth.Token)
            //{
            //}
            static int Tabs = 0;
            public string indents;
            public Parser parser;
            List<SyntaxNodeOrToken> ntl;
            public SyntaxNode node;
            public List<uint> uCwds;
            public List<int> vals;
            public uint uCwd;
            public int cwIx = 0;
            public Parser.XN xN;
            public List<Parser.XN> xNs = new List<Parser.XN>();
            public Stack<Parser.XN> xNStack = new Stack<Parser.XN>();
            public override void Visit(SyntaxNode node)
            {
                var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
                var linenum = lineSpan.StartLinePosition.Line;
                parser.lineno = linenum + 1;
                Tabs++;
                indents = new String('\t', Tabs);
                //nStk.Push(node);
                switch (node.Kind().ToString())
                {
                    case "ParenthesizedExpression":
                        Console.WriteLine(" Visit ParenthesizedExpression " + node);
                        break;
                    case "ExpressionStatement":
                        Console.WriteLine(" Visit ExpressionStatement     " + node);
                        break;
                    case "AddExpression":
                    case "SubtractExpression":
                    case "MultiplyExpression":
                        break;
                    case "IdentifierName":
                        break;
                    case "NumericLiteralExpression":
                        //Console.WriteLine(" new   NumericLiteralExpression                               " + node);
                        break;
                    case "SimpleAssignmentExpression":
                        ////Console.WriteLine("                         Visit   SimpleAssignmentExpression   " + node);
                        break;
                    case "IfStatement":
                        Console.WriteLine(node.Kind().ToString());
                        //CStmnt cIf = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "ElseClause":
                        Console.WriteLine(node.Kind().ToString());
                        //CStmnt cElse = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "ForStatement":
                        Console.WriteLine(" Visit For " + node.Kind().ToString());
                        Console.WriteLine(" set loopix = cwds.Count at end of inits ");
                        //CStmnt cFor = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "WhileStatement":
                        //Console.WriteLine(" Visit While " + node);
                        //CStmnt cWhile = new CStmnt(linenum, node);
                        //cWhile.loopix = uCwds.Count;
                        //cSStk.Push(cWhile);
                        break;
                    case "DoStatement":
                        //Console.WriteLine(node.Kind().ToString());
                        Console.WriteLine(" Visit DoWhile " + node.Kind().ToString());
                        //Console.WriteLine(" set loopix = cwds.Count, false cond continues ");
                        //CStmnt cDo = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "Block":
                        Console.WriteLine("Visit Block " + node.Kind().ToString());
                        //CStmnt cBlk = new CStmnt(linenum, node);
                        //CStmnt.CBlk cblk = new CStmnt.CBlk(cBlk, node.Parent);
                        //cSStk.Push(cBlk);
                        //ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        //if (cblk.ns.Count > 5)
                        //    Console.WriteLine("Visit Block " + node.Kind().ToString());
                        //else
                        //Console.WriteLine(" End Block updates false cond ix  " + node.Kind().ToString());
                        break;
                    default:
                        //Console.WriteLine("      Pop  " + nStk.Pop().Kind().ToString());
                        //nStk.Pop();
                        break;
                }

                /*
                        Console.WriteLine(" Visit While " + node);
                        CStmnt cWhile = new CStmnt(linenum, node);
                        CStmnt.CWhile cwhile = new CStmnt.CWhile(cWhile, node);
                        cwhile.loopix = uCwds.Count;
                        cSStk.Push(cWhile);
                        cwhile.condix = uCwds.Count;
                        cWhile.loopix = uCwds.Count;

                        xN.cmpr = xNStk.Pop().val == xN.val ? true : false;
                        xNStk.Push(xN);
                        Console.WriteLine(" Equals       " + node + " == " + xNStk.Peek().val.ToString());

                        Console.WriteLine(node.Kind().ToString() + " " + node);                        
                        xN = new XN(node);
                        xN.val = xNStk.Pop().val;
                        Console.WriteLine("             Pop " + xN.val.ToString());
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                 * 
                 */


                //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
                //                    sCwA[2]                                eop ? sCwA[cwCt.qb] : 0
                //                    sCwB[0]              sCwB[cStk.qb + 1] eop ? 0 : sCwB[cStk.qb + 1] 
                //                                         va[sCwA.qa]       pway ? pway@ 
                //                                                            : etime ? cStk.qa
                //                                         vb[pop ? cStk.qa
                //                                          : etime ? sCwA.qb 
                //                                           : sCwA.qb]                            
                //                    cwCt.qa = 2		   cwCtA = cwCt.qa + 1

                //nL.Add(node);
                base.Visit(node);
                Tabs--;
                //List<int> vals = memCpu.vRam.vals;
                switch (node.Kind().ToString())
                {
                    case "Block":
                        Console.WriteLine(" Pop Block                  " + node.Kind().ToString());
                        //CStmnt blk = cSStk.Pop();
                        //Console.WriteLine(" update false target   " + nStk.Peek().Kind());
                        //if (cSStk.Count != 0)
                        //    cSStk.Peek().endix = uCwds.Count;
                        break;
                    case "WhileStatement":
                        Console.WriteLine("  Pop WhileStatement" + node);
                        //cSStk.Peek().endix = uCwds.Count;
                        //cSStk.Pop();
                        break;
                    case "ExpressionStatement":
                        //nStk.Pop();
                        //Console.WriteLine(" Pop ExpressionStatement             " + node);
                        break;
                    case "ParenthesizedExpression":
                        Console.WriteLine(" Pop ParenthizedExpressionStatement  " + node);
                        break;
                    case "SimpleAssignmentExpression":
                        xN.vval = xNStack.Pop().val;
                        xN.node = xNStack.Pop().node;
                        xN.vix = xNs.Find(vnm => vnm.node == xN.node).vix;
                        xNs[xN.vix].val = xN.val;
                        //Console.WriteLine(" Pop   assign " + xNStack.Peek().node + " result = " + xN.val.ToString());
                        Console.WriteLine("       assign " + xN.node + ".value = " + xN.val.ToString());
                        break;
                    case "IdentifierName":
                    case "NumericLiteralExpression":
                        Console.WriteLine(" Pop   " + node.Kind() + "    " + node);
                        if ((xN = xNs.Find(vnm => vnm.node == node)) == null)
                        {
                            xN = new Parser.XN(node, xNs);
                            xN.vix = xNs.Count;
                            if (node.Kind().ToString() == "NumericLiteralExpression")
                                xN.val = (int)node.GetFirstToken().Value;
                            xNs.Add(xN);
                            //xN.vix = memCpu.vRam.vals.Count;
                            //memCpu.vRam.vals.Add(xN.val);
                            Console.WriteLine(" New    " + node + " vix = " + xN.vix.ToString() + "  Push  " + node.ToString());
                        }
                        else
                        {
                            Console.WriteLine(" Found " + node + " vix = " + xN.vix.ToString() + " Push " + node.ToString());
                        }
                        xNStack.Push(xN);
                        break;
                    case "AddExpression":
                    case "SubtractExpression":
                    case "MultiplyExpression":
                        xN = new Parser.XN(node, xNs);
                        //Stack<Parser.XN> xNStk = new Stack<Parser.XN>();

                        xN.node = node;
                        xN.val = xNStack.Pop().val;
                        Console.WriteLine(" Pop " + xN.val.ToString());
                        //RISCV.NStruct nstr = new RISCV.NStruct(xN.node, xNStack.Peek().val);
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        if (ntl[1].IsToken)
                            switch (ntl[1].AsToken().Text)
                            {
                                case "+":
                                    xN.val = xNStack.Pop().val + xN.val;
                                    Console.WriteLine(" Sum       " + node + " = " + xN.val.ToString());
                                    break;
                                case "-":
                                    xN.val = xNStack.Pop().val - xN.val;
                                    Console.WriteLine(" Difference " + node + " = " + xN.val.ToString());
                                    break;
                                case "*":
                                    xN.val = xNStack.Pop().val * xN.val;
                                    Console.WriteLine(" Product    " + node + " = " + xN.val.ToString());
                                    break;
                                default:
                                    break;
                            }
                        xNStack.Push(new Parser.XN(node, xNs));
                        xNStack.Peek().val = xN.val;
                        break;
                    case "BitwiseAndExpression":
                    case "BitwiseOrExpression":
                    case "ExclusiveOrExpression":
                        Console.WriteLine(" BinExpr         " + node);
                        break;
                    case "EqualsExpression":
                    case "NotEqualsExpression":
                    case "LessThanExpression":
                    case "GreaterThanExpression":
                    case "LessThanOrEqualExpression":
                    case "GreaterThanOrEqualExpression":
                        Console.WriteLine(node.Kind().ToString() + " " + node);
                        //xN = new Parser.XN();
                        //xN.val = xNStk.Pop().val;
                        Console.WriteLine("             Pop " + xN.val.ToString());
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        //switch (ntl[1].AsToken().ValueText)
                        //{
                        //    case "<":
                        //        xN.cmpr = xNStk.Pop().val < xN.val ? true : false;
                        //        xNStk.Push(xN);
                        //        Console.WriteLine(" LessThan    " + node + " < " + xNStk.Peek().val.ToString());
                        //        break;
                        //    case "<=":
                        //        xN.cmpr = xNStk.Pop().val <= xN.val ? true : false;
                        //        xNStk.Push(xN);
                        //        Console.WriteLine(" LessThanOrEqual    " + node + " <= " + xNStk.Peek().val.ToString());
                        //        break;
                        //    case "==":
                        //        xN.cmpr = xNStk.Pop().val == xN.val ? true : false;
                        //        xNStk.Push(xN);
                        //        Console.WriteLine(" Equals       " + node + " == " + xNStk.Peek().val.ToString());
                        //        break;
                        //    case "!=":
                        //        xN.cmpr = xNStk.Pop().val != xN.val ? true : false;
                        //        xNStk.Push(xN);
                        //        Console.WriteLine(" NotEquals " + node + " " + xN.cmpr.ToString());
                        //        break;
                        //    case ">":
                        //        xN.cmpr = xNStk.Pop().val > xN.val ? true : false;
                        //        xNStk.Push(xN);
                        //        Console.WriteLine(" GreaterThan    " + node + " > " + xNStk.Peek().val.ToString());
                        //        break;
                        //    case ">=":
                        //        xN.cmpr = xNStk.Pop().val >= xN.val ? true : false;
                        //        xNStk.Push(xN);
                        //        Console.WriteLine(" GreaterThanOrEquals    " + node + " >= " + xNStk.Peek().val.ToString());
                        //        break;
                        //    default:
                        //        break;
                        //}
                        break;


                    case "if":
                        Console.WriteLine("  IfStatement ");
                        //CStmnt cStmnt = new CStmnt(linenum, node);
                        //CStmnt.CIf cIf = new CStmnt.CIf(cStmnt, node);
                        //cIf.srcln = parser.lineno.ToString();
                        //                        sXList.AddLast(cIf);
                        //                        cIf.cond = inSB.ToString().Substring(0, inSB.ToString().IndexOf(')') + 1);
                        //                        cIf.cond = rpb.ToString();
                        //                        cIf.cwix = cas.Count;
                        //                        cwmem[cwmem.Count - 1] |= (uint) ((int)(ops.eop | ops.fna) << 16 | cIf.fcx);
                        //                        cIf.fcx = cwmem.Count;  // fcx saves cc Ix for false target
                        //                        cwmem.Add(0); // update after true statement(s)
                        //                        cwmem[cIf.fcx] = (uint) cwmem.Count; // 
                        //                        if (inSB.ToString().IndexOf("else") >= 0)
                        //                        {
                        //                            inSB.Remove(0, inSB.ToString().IndexOf("else") + 4);
                        //                            cas.Add(ccs.Count);
                        //                            cas.Add(0);
                        //                            ccs.Add((int)(ops.tna | ops.fna)); // insert jump over else 
                        //                            cIf.enx = ccs.Count; // ccs 
                        //                            ccs.Add(0);
                        //                            ccs[cIf.fcx] = cas.Count;
                        //                            ckwds(sXList, vbls, cwmem);
                        //                            ccs[cIf.enx] = cas.Count; // come here to skip else true cond
                        //                        }
                        //return cIf;
                        break;
                    case "while":
                        Console.WriteLine("  WhileStatement ");
                        //cStmnt = new CStmnt(linenum, node);
                        //CStmnt.CWhile cWhile = new CStmnt.CWhile(cStmnt, node);
                        //cWhile.srcln = lineno.ToString();
                        //                        sXList.AddLast(cWhile);
                        //                        cWhile.cond = inSB.ToString().Substring(0, inSB.ToString().IndexOf(')') + 1);
                        //                        cWhile.cond = rpb.ToString();
                        //                        //  10/5/13
                        //                        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
                        //                        //          	      sCwA[2]                                eop ? sCwA[cwCt.qa] : 0
                        //                        //					  sCwB[0]             sCwB[cwCt.qa + 1]  eop ? 0 : sCwB[cwCt.qa + 1] 
                        //                        //										  va[sCwA.qa]        pway ? pway@ : etime ? stkCt.qa]
                        //                        //										  vb[pop ? stkCt.qa : etime ? sCwB.qb : sCwA.qb]                            
                        //                        //                    cwCt.qa = 2         cwCtA = cwCt.qa + 1 

                        //                        makCond(cWhile.cond, vbls, cwmem); // blds opwds for cond eval
                        //                        cWhile.loopix = cwmem.Count - 1;
                        //                        cwmem[cwmem.Count - 1] |= (int) (ops.eop | ops.fna | ops.etime) << 16; // fcond jumps over body
                        //                        cWhile.bodyix = cwmem.Count;
                        //                        //            cWhile.condix= mem.Count;
                        //                        makCond(cWhile.cond, vbls, cwmem);// blds opwds for cond eval
                        //                       cwmem[cwmem.Count - 1] |= (uint) ((int)(ops.eop | ops.tna | ops.etime) << 16 | cWhile.bodyix); // fcond jumps over body
                        //                       //     mem.Add(0);
                        //                        cwmem[cWhile.loopix] |= (uint) cwmem.Count; ;
                        //                        //           mem[cWhile.loopix + 1] = (int)(ops.tna | ops.fna | ops.etime | ops.eop) << 16 | cWhile.condix;
                        //                        return cWhile;
                        break;

                    case "for": // inits, fcond jumps over blk and post assigns
                        Console.WriteLine("  ForStatement ");
                        //cStmnt = new CStmnt(linenum, node);
                        //CStmnt.CFor cFor = new CStmnt.CFor(cStmnt, node);
                        //                        sXList.AddLast(cFor);
                        //                              cFor.init = inSplitA[0];
                        //                              cFor.cond = inSplitA[1];
                        //                              cFor.post = inSplitA[2];
                        //                              inSB.Remove(0, tIx + 1);
                        //                          }
                        //                      else
                        //                      MessageBox.Show("for syntax");
                        //                      {
                        //                      mem_opwds(rpb.ToString(), cwmem, vbls);
                        //                      }
                        //                      // for init done:  leave space in cas to go to cond eval then true cond repeats loop
                        //                       makCond(cFor.cond, vbls, cwmem); // blds opwds for cond eval
                        //                       cwmem[cwmem.Count - 1] |= (int)(ops.eop | ops.fna | ops.etime) << 16; // fcond jumps over body
                        //                       cFor.bodyix = cwmem.Count;
                        //                       mem_opwds(rpb.ToString(), cwmem, vbls);
                        //                  }   // body and post-fix done
                        //                      cwmem[cwmem.Count - 1] |= (uint)((int)(ops.eop | ops.tna | ops.etime) << 16 | cFor.bodyix); // fcond jumps over body
                        //                      cwmem[cFor.bodyix - 1] |= (uint)cwmem.Count;
                        break;
                    case "do":
                        Console.WriteLine("  DoWhileStatement ");
                        //cStmnt = new CStmnt(linenum, node); // tcond at end to jump back
                        //                        CWhile doWhile = new CWhile();
                        //                       doWhile.loopix = cwmem.Count;  // ix of "while" cwd used to insert tcx
                        //                       doWhile.cond = rpb.ToString();
                        //                       makCond(doWhile.cond, vbls, cwmem);
                        //                      cwmem[cwmem.Count - 1] |= (uint)((int)(ops.etime | ops.eop | ops.tna) << 16 | doWhile.loopix);
                        break;
                    case "switch":
                        //cStmnt = new CStmnt(linenum, node);
                        //CStmnt.CSwitch cSwitch = new CStmnt.CSwitch(cStmnt, node);
                        break;
                    /* function is a variable that accesses variables to compute the return value for the get accessor.
                     * Call pushes arguments on the stack that grows downward in vbl memory so the address is formed by 
                     * adding the index to stack pointer.  The main function pointer is zero so the variables index is
                     * used to address variables using the same mechanism.  The call bit addresses the function using
                     * the internal variable value and returns the ALU result value instead of the internal variable value.
                     */
                    /* push base | stkct to use as part of call to pass args  call push pway
                     * push args   push pway
                     * push rtnct | cwct 
                     * stkct to base, cwct =fun@   call endop cwct[ocwB]
                     * dofnctn
                     * restore counts wrt return to TOS call pop
                     */
                    //               case "fncall":
                    //                    cS = new CStmnt();
                    //                   cS.srcln = lineno.ToString();
                    //                   cS.name = "funcall";
                    //                   sXList.AddLast(cS);
                    //                   if ((lp = inSB.ToString().IndexOf('(')) >= 0)
                    //                   else
                    //                   MessageBox.Show("call parens error");
                    //                   cS.xprn = cvbls[get_vbl(inSB.ToString().Substring(0, lp).Trim(), vbls)].cfnref.sb.ToString();
                    //                  }
                    //                  else
                    //                  MessageBox.Show("call parens error");
                    //                  break;
                    //                case "vasgn":
                    //                     cStmnt = new CStmnt();
                    //                     mem_opwds(rpb.ToString(), cwmem, vbls);
                    //                    sXList.AddLast(cStmnt);
                    //                 return cStmnt;
                    default:
                        break;
                } // end while switch                 else    //  eval


                */

               //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
               // cwCt = 0	          mem[cwCt.qa]                            mem[cwCt.qa]
               //					                      mem[cwCt.qa + 1]                       mem[cwCt.qa + 1]           
               //										  va[sCw.qb]          vA[op1@, stkCt.qa]
               //										  vb[oCw.qb]          vB[oCw.qb]
               // cwCtA = 2		  cwCtA = nxtScw                                                                    cwCtA = nxtScw
               //                    va,vb addr          op1, op2, ocw       TOS, op2, ocw


               base.Visit(node);
                Tabs--;
            }

        }
        //public void begin(String fPath, ListBox lB1in)  //  , CEngine pE
        //{
        //    lB1 = lB1in;
        //    lineno = 0;
        //    //StreamReader sR = new StreamReader(@"C:\Windows.old.001\Users\Karl\Dropbox\ASTEngine\ASTDemo.cs");
        //    //sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
        //    sB = new StringBuilder("main() { y = 2 + 3 * 4 - 4; }");
        //    CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
        //    node = tree.GetRoot();
        //    CEngineWalker cEwalker = new CEngineWalker();
        //    cEwalker.parser = this;
        //    cEwalker.uCwds = uCwds;
        //    cEwalker.Visit(node);
        //}


        public class XN
        {
            public XN(SyntaxNode node, List<XN> cvblsA)
            {
                this.node = node;
                vix = cvblsA.Count;
            }
            public int val
            {
                get { return vval; }
                set { vval = value; }
            }
            public SyntaxNode node; public int vval; public string type;
            public int scn_ix; public int vix = -1;
        }

        public class CStmnt
        {
            public int cwix, srcln, loopix;
            public string name, xprn, kwd, cond;
            public StringBuilder expSB;

            public CStmnt()
            { }
            public CStmnt(int lno, String k)
            {
                srcln = lno;
                kwd = k.Trim();
                //expSB = new StringBuilder(x); // build cond, exprn strings
            }
            public class CIf
            {
                CStmnt parent;
                public CIf(CStmnt p, SyntaxNode n)
                { parent = p; }
                public LinkedList<CStmnt> tList = new LinkedList<CStmnt>();
                public LinkedList<CStmnt> fList = new LinkedList<CStmnt>();
                //public int condix;
                public int tcx;
                public int fcx;
                public int enx;
                public String ckwd { get { return parent.kwd; } }
                public String lineno { get { return parent.srcln.ToString(); } }
            }
            public class CFor
            {
                CStmnt parent;
                public CFor(CStmnt p, SyntaxNode n)
                { parent = p; }
                public List<CStmnt> preAsgn = new List<CStmnt>();
                public List<CStmnt> postAsgn = new List<CStmnt>();
                public string init, post;
                public string cond { get { return parent.cond; } }
                public LinkedList<CStmnt> fList = new LinkedList<CStmnt>();
                public CStmnt initref;
                public CStmnt tref;
                public CStmnt endref;
                public int condix;
                public int bodyix, myLoopix;
                public int loopix { get { return myLoopix; } set { myLoopix = value; } }
                public int tcx;
                public int fcx;
                public String ckwd { get { return parent.kwd; } }
                public String lineno { get { return parent.srcln.ToString(); } }
            }
            public class CWhile
            {
                CStmnt parent;
                SyntaxNode Node;
                public CWhile(CStmnt p, SyntaxNode n)
                {
                    parent = p;
                    Node = n;
                }
                public string ckwd { get { return "while"; } }
                //public new string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
                //public new string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
                public int lineno { get { return parent.srcln; } set { parent.srcln = value; } }

                public int condix, bodyix, endix, loopix;
            }
            public class CDoWhile
            {
                CStmnt parent;
                SyntaxNode Node;
                public CDoWhile(CStmnt p, SyntaxNode n)
                {
                    parent = p;
                    Node = n;
                }
                public string ckwd { get { return "dowhile"; } }
                //public new string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
                //public new string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
                public int lineno { get { return parent.srcln; } set { parent.srcln = value; } }
                public int loopix;
                public int condix, bodyix, endix;
            }
            public class CSwitch
            {
                CStmnt parent;
                public CSwitch(CStmnt p, SyntaxNode n)
                { this.parent = p; }
                public List<CStmnt> preAsgn = new List<CStmnt>();
                public List<CStmnt> postAsgn = new List<CStmnt>();
                public String ckwd { get { return parent.kwd; } }
                public int lineno { get { return parent.srcln; } }

            }
        //}
    }
}

    public class Parser
    {
        public CEngineWalker cEWalker = new CEngineWalker();

        //public AST101.MemCpu memCpu = new AST101.MemCpu();
        public List<uint> uCwds;
        public Rop tRop;
        public int lineno;
        public int cbcnt = 0;
        public ListBox lB1;
        public XS xS;

        public class Rop
        {
            public Rop(string soper)
            { oper = soper; }
            public string oper;
            public string op1;
            public string op2;
            public int prec;
            public int opcw;
        }
        public Stack<Rop> opStk = new Stack<Rop>();
        SyntaxNode node;
        public class CEngineWalker : CSharpSyntaxWalker
        {
            //NOTE: Make sure you invoke the base constructor with 
            //the correct SyntaxWalkerDepth. Otherwise VisitToken()
            //will never get run.
            //public CEngineWalker() : base(SyntaxWalkerDepth.Token)
            //{
            //}
            static int Tabs = 0;
            public string indents;
            public Parser parser;
            //public Stack<SyntaxNodeOrToken> xStk = new Stack<SyntaxNodeOrToken>();
            //List<int> vtosv = new List<int>();
            VN vN;
            public List<VN> xNs = new List<VN>();
            //public List<XT> xTs = new List<XT>();
            List<SyntaxNodeOrToken> ntl;
            //List<SyntaxNode> nL = new List<SyntaxNode>();
            //public Stack<XT> xTStk = new Stack<XT>();
            public Stack<VN> vNStk = new Stack<VN>();
            //public SyntaxToken token = new SyntaxToken()
            //public Stack<SyntaxNode> nStk = new Stack<SyntaxNode>();
            //public Queue<SyntaxNode> nQ = new Queue<SyntaxNode>();
            //public Queue<XN> xnQ = new Queue<XN>();
            public Stack<CStmnt> cSStk = new Stack<CStmnt>();
            //public Queue<XT> xtQ = new Queue<XT>();
            public SyntaxNode node;
            public List<uint> uCwds;
            public List<int> vals;
            public uint uCwd;
            public int cwIx = 0;
            public override void Visit(SyntaxNode node)
            {
                var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
                var linenum = lineSpan.StartLinePosition.Line;
                parser.lineno = linenum + 1;
                Tabs++;
                indents = new String('\t', Tabs);
                //nStk.Push(node);
                switch (node.Kind().ToString())
                {
                    case "ParenthesizedExpression":
                        Console.WriteLine(" Visit ParenthesizedExpression " + node);
                        break;
                    case "ExpressionStatement":
                        Console.WriteLine(" Visit ExpressionStatement     " + node);
                        break;
                    case "AddExpression":
                    case "SubtractExpression":
                    case "MultiplyExpression":
                        break;
                    case "IdentifierName":
                        break;
                    case "NumericLiteralExpression":
                        //Console.WriteLine(" new   NumericLiteralExpression                               " + node);
                        break;
                    case "SimpleAssignmentExpression":
                        ////Console.WriteLine("                         Visit   SimpleAssignmentExpression   " + node);
                        break;
                    case "IfStatement":
                        Console.WriteLine(node.Kind().ToString());
                        //CStmnt cIf = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "ElseClause":
                        Console.WriteLine(node.Kind().ToString());
                        //CStmnt cElse = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "ForStatement":
                        Console.WriteLine(" Visit For " + node.Kind().ToString());
                        Console.WriteLine(" set loopix = cwds.Count at end of inits ");
                        //CStmnt cFor = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "WhileStatement":
                        Console.WriteLine(" Visit While " + node);
                        CStmnt cWhile = new CStmnt(linenum, node);
                        cWhile.loopix = uCwds.Count;
                        cSStk.Push(cWhile);
                        List<SyntaxNodeOrToken> ntl2 = new List<SyntaxNodeOrToken>(cWhile.nTs[2].ChildNodesAndTokens());   // condition list
                        List<SyntaxNodeOrToken> ntl4 = new List<SyntaxNodeOrToken>(cWhile.nTs[4].ChildNodesAndTokens());  // expression list
                        parser.cKwds(node, vals, uCwds);
                        /*
                                Console.WriteLine(" Visit While " + node);
                                CStmnt cWhile = new CStmnt(linenum, node);
                                cSStk.Push(cWhile);
                                cWhile.loopix = uCwds.Count;
                                cWhile.condix = uCwds.Count;
                                xN.cmpr = xNStk.Pop().val == xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" Equals       " + node + " == " + xNStk.Peek().val.ToString());

                                Console.WriteLine(node.Kind().ToString() + " " + node);                        
                                xN = new XN(node);
                                xN.val = xNStk.Pop().val;
                                Console.WriteLine("             Pop " + xN.val.ToString());
                                ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                         * 
                         */

                        break;
                    case "DoStatement":
                        //Console.WriteLine(node.Kind().ToString());
                        Console.WriteLine(" Visit DoWhile " + node.Kind().ToString());
                        //Console.WriteLine(" set loopix = cwds.Count, false cond continues ");
                        //CStmnt cDo = new CStmnt(linenum, node.Kind().ToString());
                        break;
                    case "Block":
                        Console.WriteLine("Visit Block " + node.Kind().ToString());
                        CStmnt cBlk = new CStmnt(linenum, node);
                        //CStmnt.CBlk cblk = new CStmnt.CBlk(cBlk, node.Parent);
                        cSStk.Push(cBlk);
                        //ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        //if (cblk.ns.Count > 5)
                        //    Console.WriteLine("Visit Block " + node.Kind().ToString());
                        //else
                        //Console.WriteLine(" End Block updates false cond ix  " + node.Kind().ToString());
                        break;
                    default:
                        //Console.WriteLine("      Pop  " + nStk.Pop().Kind().ToString());
                        //nStk.Pop();
                        break;
                }

                /*
                        Console.WriteLine(" Visit While " + node);
                        CStmnt cWhile = new CStmnt(linenum, node);
                        CStmnt.CWhile cwhile = new CStmnt.CWhile(cWhile, node);
                        cwhile.loopix = uCwds.Count;
                        cSStk.Push(cWhile);
                        cwhile.condix = uCwds.Count;
                        cWhile.loopix = uCwds.Count;

                        xN.cmpr = xNStk.Pop().val == xN.val ? true : false;
                        xNStk.Push(xN);
                        Console.WriteLine(" Equals       " + node + " == " + xNStk.Peek().val.ToString());

                        Console.WriteLine(node.Kind().ToString() + " " + node);                        
                        xN = new XN(node);
                        xN.val = xNStk.Pop().val;
                        Console.WriteLine("             Pop " + xN.val.ToString());
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                 * 
                 */


                //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
                //                    sCwA[2]                                eop ? sCwA[cwCt.qb] : 0
                //                    sCwB[0]              sCwB[cStk.qb + 1] eop ? 0 : sCwB[cStk.qb + 1] 
                //                                         va[sCwA.qa]       pway ? pway@ 
                //                                                            : etime ? cStk.qa
                //                                         vb[pop ? cStk.qa
                //                                          : etime ? sCwA.qb 
                //                                           : sCwA.qb]                            
                //                    cwCt.qa = 2		   cwCtA = cwCt.qa + 1

                //nL.Add(node);
                base.Visit(node);
                Tabs--;
                //List<int> vals = memCpu.vRam.vals;
                switch (node.Kind().ToString())
                {
                    case "Block":
                        Console.WriteLine(" Pop Block                  " + node.Kind().ToString());
                        CStmnt blk = cSStk.Pop();
                        //Console.WriteLine(" update false target   " + nStk.Peek().Kind());
                        if (cSStk.Count != 0)
                            cSStk.Peek().endix = uCwds.Count;
                        break;
                    case "WhileStatement":
                        Console.WriteLine("  Pop WhileStatement" + node);
                        cSStk.Peek().endix = uCwds.Count;
                        cSStk.Pop();
                        break;
                    case "ExpressionStatement":
                        //nStk.Pop();
                        //Console.WriteLine(" Pop ExpressionStatement             " + node);
                        break;
                    case "ParenthesizedExpression":
                        Console.WriteLine(" Pop ParenthizedExpressionStatement  " + node);
                        break;
                    case "SimpleAssignmentExpression":
                        xNStk.Pop();
                        xNs.Find(vnm => vnm.name == xNStk.Peek().name).val = xN.val;
                        Console.WriteLine(" Pop                  assign " + xNStk.Peek().node + " result = " + xN.val.ToString());
                        //Console.WriteLine("                                         assign " + xN.name + ",                      result = " + xN.val.ToString());
                        break;
                    case "IdentifierName":
                    case "NumericLiteralExpression":
                        Console.WriteLine(" Pop   " + node.Kind() + "    " + node);
                        //nStk.Pop();
                        //token = node.GetFirstToken();

                        if ((xN = xNs.Find(vnm => vnm.name == node.ToString())) == null)
                        {
                            xN = new VN(node);
                            xN.vix = xNs.Count;
                            xNs.Add(xN);
                            //xN.vix = memCpu.vRam.vals.Count;
                            //memCpu.vRam.vals.Add(xN.val);
                            xNStk.Push(xN);
                            Console.WriteLine(" New    " + node + " vix = " + xN.vix.ToString() + "  Push  " + node.ToString());
                        }
                        else
                        {
                            xNStk.Push(xN);
                            Console.WriteLine(" Found " + node + " vix = " + xN.vix.ToString() + " Push " + node.ToString());
                        }
                        break;
                    case "AddExpression":
                    case "SubtractExpression":
                    case "MultiplyExpression":
                        //xN = new XN(node);
                        xN.node = node;
                        xN.val = xNStk.Pop().val;
                        Console.WriteLine(" Pop " + xN.val.ToString());
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        //eval(ntl);
                        switch (ntl[1].AsToken().ValueText)
                        {
                            case "+":
                                xN.val = xNStk.Pop().val + xN.val;
                                Console.WriteLine(" Sum       " + node + " = " + xN.val.ToString());
                                break;
                            case "-":
                                xN.val = xNStk.Pop().val - xN.val;
                                Console.WriteLine(" Difference " + node + " = " + xN.val.ToString());
                                break;
                            case "*":
                                xN.val = xNStk.Pop().val * xN.val;
                                Console.WriteLine(" Product    " + node + " = " + xN.val.ToString());
                                break;
                            default:
                                break;
                        }
                        xNStk.Push(new VN(node));
                        xNStk.Peek().val = xN.val;
                        break;
                    case "BitwiseAndExpression":
                    case "BitwiseOrExpression":
                    case "ExclusiveOrExpression":
                        Console.WriteLine(" BinExpr         " + node);
                        break;
                    case "EqualsExpression":
                    case "NotEqualsExpression":
                    case "LessThanExpression":
                    case "GreaterThanExpression":
                    case "LessThanOrEqualExpression":
                    case "GreaterThanOrEqualExpression":
                        Console.WriteLine(node.Kind().ToString() + " " + node);
                        xN = new VN(node);
                        xN.val = xNStk.Pop().val;
                        Console.WriteLine("             Pop " + xN.val.ToString());
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        switch (ntl[1].AsToken().ValueText)
                        {
                            case "<":
                                xN.cmpr = xNStk.Pop().val < xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" LessThan    " + node + " < " + xNStk.Peek().val.ToString());
                                break;
                            case "<=":
                                xN.cmpr = xNStk.Pop().val <= xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" LessThanOrEqual    " + node + " <= " + xNStk.Peek().val.ToString());
                                break;
                            case "==":
                                xN.cmpr = xNStk.Pop().val == xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" Equals       " + node + " == " + xNStk.Peek().val.ToString());
                                break;
                            case "!=":
                                xN.cmpr = xNStk.Pop().val != xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" NotEquals " + node + " " + xN.cmpr.ToString());
                                break;
                            case ">":
                                xN.cmpr = xNStk.Pop().val > xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" GreaterThan    " + node + " > " + xNStk.Peek().val.ToString());
                                break;
                            case ">=":
                                xN.cmpr = xNStk.Pop().val >= xN.val ? true : false;
                                xNStk.Push(xN);
                                Console.WriteLine(" GreaterThanOrEquals    " + node + " >= " + xNStk.Peek().val.ToString());
                                break;
                            default:
                                break;
                        }
                        break;


                    case "if":
                        Console.WriteLine("  IfStatement ");
                        CStmnt cStmnt = new CStmnt(linenum, node);
                        CStmnt.CIf cIf = new CStmnt.CIf(cStmnt, node);
                        //cIf.srcln = parser.lineno.ToString();
                        //                        sXList.AddLast(cIf);
                        //                        cIf.cond = inSB.ToString().Substring(0, inSB.ToString().IndexOf(')') + 1);
                        //                        cIf.cond = rpb.ToString();
                        //                        cIf.cwix = cas.Count;
                        //                        cwmem[cwmem.Count - 1] |= (uint) ((int)(ops.eop | ops.fna) << 16 | cIf.fcx);
                        //                        cIf.fcx = cwmem.Count;  // fcx saves cc Ix for false target
                        //                        cwmem.Add(0); // update after true statement(s)
                        //                        cwmem[cIf.fcx] = (uint) cwmem.Count; // 
                        //                        if (inSB.ToString().IndexOf("else") >= 0)
                        //                        {
                        //                            inSB.Remove(0, inSB.ToString().IndexOf("else") + 4);
                        //                            cas.Add(ccs.Count);
                        //                            cas.Add(0);
                        //                            ccs.Add((int)(ops.tna | ops.fna)); // insert jump over else 
                        //                            cIf.enx = ccs.Count; // ccs 
                        //                            ccs.Add(0);
                        //                            ccs[cIf.fcx] = cas.Count;
                        //                            ckwds(sXList, vbls, cwmem);
                        //                            ccs[cIf.enx] = cas.Count; // come here to skip else true cond
                        //                        }
                        //return cIf;
                        break;
                    case "while":
                        Console.WriteLine("  WhileStatement ");
                        cStmnt = new CStmnt(linenum, node);
                        //CStmnt.CWhile cWhile = new CStmnt.CWhile(cStmnt, node);
                        //cWhile.srcln = lineno.ToString();
                        //                        sXList.AddLast(cWhile);
                        //                        cWhile.cond = inSB.ToString().Substring(0, inSB.ToString().IndexOf(')') + 1);
                        //                        cWhile.cond = rpb.ToString();
                        //                        //  10/5/13
                        //                        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
                        //                        //          	      sCwA[2]                                eop ? sCwA[cwCt.qa] : 0
                        //                        //					  sCwB[0]             sCwB[cwCt.qa + 1]  eop ? 0 : sCwB[cwCt.qa + 1] 
                        //                        //										  va[sCwA.qa]        pway ? pway@ : etime ? stkCt.qa]
                        //                        //										  vb[pop ? stkCt.qa : etime ? sCwB.qb : sCwA.qb]                            
                        //                        //                    cwCt.qa = 2         cwCtA = cwCt.qa + 1 

                        //                        makCond(cWhile.cond, vbls, cwmem); // blds opwds for cond eval
                        //                        cWhile.loopix = cwmem.Count - 1;
                        //                        cwmem[cwmem.Count - 1] |= (int) (ops.eop | ops.fna | ops.etime) << 16; // fcond jumps over body
                        //                        cWhile.bodyix = cwmem.Count;
                        //                        //            cWhile.condix= mem.Count;
                        //                        makCond(cWhile.cond, vbls, cwmem);// blds opwds for cond eval
                        //                       cwmem[cwmem.Count - 1] |= (uint) ((int)(ops.eop | ops.tna | ops.etime) << 16 | cWhile.bodyix); // fcond jumps over body
                        //                       //     mem.Add(0);
                        //                        cwmem[cWhile.loopix] |= (uint) cwmem.Count; ;
                        //                        //           mem[cWhile.loopix + 1] = (int)(ops.tna | ops.fna | ops.etime | ops.eop) << 16 | cWhile.condix;
                        //                        return cWhile;
                        break;

                    case "for": // inits, fcond jumps over blk and post assigns
                        Console.WriteLine("  ForStatement ");
                        cStmnt = new CStmnt(linenum, node);
                        CStmnt.CFor cFor = new CStmnt.CFor(cStmnt, node);
                        //                        sXList.AddLast(cFor);
                        //                              cFor.init = inSplitA[0];
                        //                              cFor.cond = inSplitA[1];
                        //                              cFor.post = inSplitA[2];
                        //                              inSB.Remove(0, tIx + 1);
                        //                          }
                        //                      else
                        //                      MessageBox.Show("for syntax");
                        //                      {
                        //                      mem_opwds(rpb.ToString(), cwmem, vbls);
                        //                      }
                        //                      // for init done:  leave space in cas to go to cond eval then true cond repeats loop
                        //                       makCond(cFor.cond, vbls, cwmem); // blds opwds for cond eval
                        //                       cwmem[cwmem.Count - 1] |= (int)(ops.eop | ops.fna | ops.etime) << 16; // fcond jumps over body
                        //                       cFor.bodyix = cwmem.Count;
                        //                       mem_opwds(rpb.ToString(), cwmem, vbls);
                        //                  }   // body and post-fix done
                        //                      cwmem[cwmem.Count - 1] |= (uint)((int)(ops.eop | ops.tna | ops.etime) << 16 | cFor.bodyix); // fcond jumps over body
                        //                      cwmem[cFor.bodyix - 1] |= (uint)cwmem.Count;
                        break;
                    case "do":
                        Console.WriteLine("  DoWhileStatement ");
                        cStmnt = new CStmnt(linenum, node); // tcond at end to jump back
                                                            //                        CWhile doWhile = new CWhile();
                                                            //                       doWhile.loopix = cwmem.Count;  // ix of "while" cwd used to insert tcx
                                                            //                       doWhile.cond = rpb.ToString();
                                                            //                       makCond(doWhile.cond, vbls, cwmem);
                                                            //                      cwmem[cwmem.Count - 1] |= (uint)((int)(ops.etime | ops.eop | ops.tna) << 16 | doWhile.loopix);
                        break;
                    case "switch":
                        cStmnt = new CStmnt(linenum, node);
                        CStmnt.CSwitch cSwitch = new CStmnt.CSwitch(cStmnt, node);
                        break;
                    /* function is a variable that accesses variables to compute the return value for the get accessor.
                     * Call pushes arguments on the stack that grows downward in vbl memory so the address is formed by 
                     * adding the index to stack pointer.  The main function pointer is zero so the variables index is
                     * used to address variables using the same mechanism.  The call bit addresses the function using
                     * the internal variable value and returns the ALU result value instead of the internal variable value.
                     */
                    /* push base | stkct to use as part of call to pass args  call push pway
                     * push args   push pway
                     * push rtnct | cwct 
                     * stkct to base, cwct =fun@   call endop cwct[ocwB]
                     * dofnctn
                     * restore counts wrt return to TOS call pop
                     */
                    //               case "fncall":
                    //                    cS = new CStmnt();
                    //                   cS.srcln = lineno.ToString();
                    //                   cS.name = "funcall";
                    //                   sXList.AddLast(cS);
                    //                   if ((lp = inSB.ToString().IndexOf('(')) >= 0)
                    //                   else
                    //                   MessageBox.Show("call parens error");
                    //                   cS.xprn = cvbls[get_vbl(inSB.ToString().Substring(0, lp).Trim(), vbls)].cfnref.sb.ToString();
                    //                  }
                    //                  else
                    //                  MessageBox.Show("call parens error");
                    //                  break;
                    //                case "vasgn":
                    //                     cStmnt = new CStmnt();
                    //                     mem_opwds(rpb.ToString(), cwmem, vbls);
                    //                    sXList.AddLast(cStmnt);
                    //                 return cStmnt;
                    default:
                        break;
                } // end while switch 

            }
            //public override void VisitToken(SyntaxToken token)  // called by VisitNode
            //{
            //    var lineSpan = token.SyntaxTree.GetLineSpan(token.Span);
            //    var linenum = lineSpan.StartLinePosition.Line;
            //    parser.lineno = linenum + 1;
            //    var indents = new String('\t', Tabs);
            //    if (token.ValueText == "main")
            //        return;
            //    switch (token.Kind().ToString())
            //    {
            //        case "NumericLiteralToken":
            //        case "IdentifierToken":
            //            if ((xT = xTs.Find(vnm => vnm.name == token.ValueText)) == null)
            //            {
            //                xT = new XT(token);
            //                xTs.Add(xT);
            //                xT.vix = memCpu.vRam.vals.Count;
            //                memCpu.vRam.vals.Add(xT.val);
            //                Console.WriteLine("                  New xTkn                   " + token.ValueText + " vix = " + xT.vix);
            //                //    xtQ.Enqueue(xT);
            //            }
            //            else
            //            {
            //                Console.WriteLine("                 Found xTkn                 " + token.ValueText + " vix = " + xT.vix);
            //            }
            //            //var tnode = token.Parent;
            //            //List<int> il = memCpu.vRam.vals;
            //            break;
            //        case "main":
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }


        public class VN
        {
            public VN(SyntaxNode sN)
            {
                //this.token = tkn.ToString();
                node = sN;
                if (sN.Kind().ToString() == "NumericLiteralExpression")
                {
                    vval = Int32.Parse(sN.ToString());
                    asgnd = true;
                }
            }
            public string name
            {
                get { return node.ToString(); }
            }

            public int val
            {
                get { return vval; }
                set
                {
                    if (node.Kind().ToString() != "NumericLiteralExpression")
                    {
                        vval = value;
                        asgnd = true;
                    }
                }
            }
            public string token;
            public SyntaxNode node;
            public int vval, scn_ix, vix = -1;
            public bool asgnd = false, cmpr;

        }


        //public class XT
        //{
        //    public XT(SyntaxToken tkn)
        //    {
        //        this.token = tkn;
        //        this.node = tkn.Parent;
        //        if (char.IsDigit(tkn.ValueText[0]))
        //        {
        //            vval = (int)tkn.Value;
        //            asgnd = true;
        //        }
        //    }
        //    public string name
        //    {
        //        get { return token.ValueText; }
        //    }

        //    public virtual int val
        //    {
        //        get { return char.IsLetter(token.ValueText[0]) ? vval : (int)token.Value; }
        //        set
        //        {
        //            vval = value;
        //            asgnd = true;
        //        }
        //    }
        //    public SyntaxToken token;
        //    public SyntaxNode node;
        //    public int vval, scn_ix, vix = -1;
        //    public bool asgnd = false;
        //}


        public void cKwds(SyntaxNode node, List<int> vbls, List<uint> uCwds)
        {
            CStmnt cS = new CStmnt(lineno, node);
            cS.name = node.Kind().ToString();
            //int lp = 0, rp = 0;
            char[] semi = new char[] { ';' };
            char[] keychars = new char[] { 'i', 'e', 'f', 'w', 'd', 's' };
            string[] keywords = new string[] { "=", "(", "{", "}", "if", "else", "for", "while", "do", "switch" };

            switch (node.Kind().ToString().Trim())
            {
                case "if":
                    CStmnt.CIf cIf = new CStmnt.CIf(cS, node);
                    //cIf.cond = sBin.ToString().Substring(0, sBin.ToString().IndexOf(')') + 1);
                    //rplSB(cIf.cond);
                    //sBin.Remove(0, cIf.cond.Length);
                    //cIf.cond = rpb.ToString();
                    //makCond(cIf.cond, vbls, uCwds);
                    uCwds[uCwds.Count - 1] |= (uint)((int)(UcdEnum.eop | UcdEnum.fna) | cIf.fcx);
                    cIf.fcx = uCwds.Count;  // fcx saves cc Ix for false target
                    uCwds.Add(0); // update after true statement(s)
                                  //nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);  // ckwds(sXList);  // cIf.tList);
                    uCwds[cIf.fcx] = (uint)uCwds.Count; // 
                                                        //if (sBin.ToString().IndexOf("else") >= 0)
                                                        //{
                                                        //    sBin.Remove(0, sBin.ToString().IndexOf("else") + 4);
                                                        //    //cas.Add(ccs.Count);
                                                        //    //cas.Add(0);
                                                        //    //ccs.Add((int)(ucdEnum.tna | ucdEnum.fna)); // insert jump over else 
                                                        //    //cIf.enx = ccs.Count; // ccs 
                                                        //    //ccs.Add(0);
                                                        //    //ccs[cIf.fcx] = cas.Count;
                                                        //    //cKwds(sXList, vbls, Scws);
                                                        //    //ccs[cIf.enx] = cas.Count; // come here to skip else true cond
                                                        //}
                    return;

                //  10/5/13
                //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
                //          	      sCwA[2]                                eop ? sCwA[cwCt.qa] : 0
                //					  sCwB[0]             sCwB[cwCt.qa + 1]  eop ? 0 : sCwB[cwCt.qa + 1] 
                //										  va[sCwA.qa]        pway ? pway@ : etime ? stkCt.qa]
                //										  vb[pop ? stkCt.qa : etime ? sCwB.qb : sCwA.qb]                            
                //                    cwCt.qa = 2         cwCtA = cwCt.qa + 1 


                //    cWhile.cond = rpb.ToString();
                //    makCond(cWhile.cond, vbls, Scws); // blds opwds for cond eval
                //    Scws[Scws.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime; // fcond jumps over body
                //    cWhile.bodyix = Scws.Count;
                //    delimIx = nxtDelim();
                //    nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
                //    Scws.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cWhile.bodyix)); // fcond jumps over body
                //    Scws[cWhile.loopix] |= (uint)Scws.Count; ;
                case "WhileStatement":
                    CStmnt.CWhile cWhile = new CStmnt.CWhile(cS, node);
                    //    cWhile.loopix = Scws.Count - 1;
                    cWhile.loopix = uCwds.Count;
                    uCwds.Add((uint)((int)(UcdEnum.eop | UcdEnum.tna | UcdEnum.etime) | cWhile.fIx)); // fcond jumps over body
                    cWhile.loopix = uCwds.Count - 1;
                    uCwds[uCwds.Count - 1] |= (int)(UcdEnum.eop | UcdEnum.fna | UcdEnum.etime); // fcond jumps over body
                                                                                                //cWhile.bodyix = uCwds.Count;
                    uCwds.Add((uint)((int)(UcdEnum.eop | UcdEnum.tna | UcdEnum.etime) | cWhile.fIx)); // fcond jumps over body
                    uCwds[cWhile.loopix] |= (uint)uCwds.Count;
                    //uCwds[cWhile.loopix] |= (uint)uCwds.Count; ;
                    return;
                //case "while":
                //    CWhile cWhile = (CWhile)sXList.Last.Value;
                //    rplSB(cWhile.cond);
                //    cWhile.cond = rpb.ToString();
                //    makCond(cWhile.cond, vbls, Scws); // blds opwds for cond eval
                //    cWhile.loopix = Scws.Count - 1;
                //    Scws[Scws.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime; // fcond jumps over body
                //    cWhile.bodyix = Scws.Count;
                //    delimIx = nxtDelim();
                //    nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
                //    Scws.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cWhile.bodyix)); // fcond jumps over body
                //    Scws[cWhile.loopix] |= (uint)Scws.Count; ;
                //    return cWhile;

                case "for": // inits, fcond jumps over blk and post assigns
                    CStmnt.CFor cFor = new CStmnt.CFor(cS, node);
                    //inSplitA = cFor.xprn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (string pfx in inSplitA) // 0 or more assignments
                    //{
                    //    //rplSB(pfx + ';');
                    //    //mem_opwds(rpb.ToString(), uCwds, vbls);
                    //}
                    // for init done:  leave space in cas to go to cond eval then true cond repeats loop
                    //rplSB(cFor.cond);
                    //cFor.cond = rpb.ToString().Trim();
                    //makCond(cFor.cond, vbls, uCwds); // blds opwds for cond eval
                    uCwds[uCwds.Count - 1] |= (int)(UcdEnum.eop | UcdEnum.fna | UcdEnum.etime); // fcond jumps over body
                    cFor.bodyix = uCwds.Count;
                    //delimIx = nxtDelim();
                    //nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
                    //inSplitA = cFor.post.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    //foreach (string pfx in inSplitA)
                    //{
                    //rplSB(pfx + ';');
                    //mem_opwds(rpb.ToString(), uCwds, vbls);
                    //nxtDelim();
                    //} // body and post-fix done
                    uCwds[uCwds.Count - 1] |= (uint)((int)(UcdEnum.eop | UcdEnum.tna | UcdEnum.etime) | cFor.bodyix); // fcond jumps over body
                    uCwds[cFor.bodyix - 1] |= (uint)uCwds.Count;
                    return;
                case "do": // tcond at end to jump back
                           //CStmnt.doWhile cFor = new CStmnt.
                           //doWhile.cond = rpb.ToString();
                           //makCond(doWhile.cond, vbls, uCwds);
                           //uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.etime | ucdEnum.eop | ucdEnum.tna) | doWhile.loopix);
                    return;
                case "switch":
                    CStmnt.CSwitch cSwitch = new CStmnt.CSwitch(cS, node);
                    //cSwitch.xprn = inSplitA[0];
                    //cSwitch.srcln = lineno;
                    //sXList.AddLast(cSwitch);
                    //MessageBox.Show("Switch expr error");
                    //getNext();
                    break;
                /* function is a variable that accesses variables to compute the return value for the get accessor.
                 * Call pushes arguments on the stack that grows downward in vbl memory so the address is formed by 
                 * adding the index to stack pointer.  The main function pointer is zero so the variables index is
                 * used to address variables using the same mechanism.  The call bit addresses the function using
                 * the internal variable value and returns the ALU result value instead of the internal variable value.
                 */
                /* push base | stkct to use as part of call to pass args  call push pway
                 * push args   push pway
                 * push rtnct | cwct 
                 * stkct to base, cwct =fun@   call endop cwct[ocwB]
                 * dofnctn
                 * restore counts wrt return to TOS call pop
                 */
                //case "fncall":
                //    cS = new CStmnt(lineno, "funcall");
                //    sXList.AddLast(cS);
                //    if ((lp = sBin.ToString().IndexOf('(')) >= 0)
                //        sBin.Remove(0, lp + 1);
                //    else
                //        MessageBox.Show("call parens error");
                //    if ((rp = sBin.ToString().IndexOf(')')) >= 0)
                //    {
                //        cS.xprn = cvbls[get_vbl(sBin.ToString().Substring(0, lp).Trim(), vbls)].cfnref.sb.ToString();
                //        sBin.Remove(0, rp + 1);
                //    }
                //    else
                //        MessageBox.Show("call parens error");
                //    if ((rp = sBin.ToString().IndexOf(')')) >= 0)
                //        sBin.Remove(0, rp + 1);
                //    break;
                //case "vasgn":
                //    cStmnt = new CStmnt();
                //    //rpb.Remove(0, rpb.Length);
                //    //rplSB(sBin.ToString().Substring(0, sBin.ToString().IndexOf(';')));
                //    mem_opwds(rpb.ToString(), uCwds, vbls);
                //    cStmnt.xprn = rpb.ToString();
                //    sXList.AddLast(cStmnt);
                //    sBin.Remove(0, sBin.ToString().IndexOf(';') + 1);
                //    //nxtDelim();
                //    return;
                default:
                    break;
            } // end while switch 
            return;
        }
        // end cKwds

        //public void mem_opwds(String rpnStr, List<uint> Scws, List<Cvbl> cvblsA)
        //{
        //    rop sx = new rop();
        //    Stack<string> xStk = new Stack<string>();
        //    StringBuilder sb1 = new StringBuilder(rpnStr);
        //    StringBuilder sb2 = new StringBuilder();
        //    string[] strOps = new string[] { " +", " -", "!", "~", "=", "?", ":", "&", "|", "^", " *", "/", "<", ">", "%" };
        //    string[] xopsA;
        //    string[] xopnds, xoper;
        //    string[] wSp = new string[] { };
        //while (sb1.Length > 0)
        //{
        //    xopsA = sb1.ToString().Split(expOps, 2, StringSplitOptions.RemoveEmptyEntries);
        //    xopnds = xopsA[0].Split(wsp, StringSplitOptions.RemoveEmptyEntries);
        //    sb1.Remove(0, xopsA[0].Length);
        //    xoper = sb1.ToString().Split(wsp, 2, StringSplitOptions.RemoveEmptyEntries);
        //    sb1.Remove(0, xoper.Length > 0 ? xoper[0].Length : 0);
        //    switch (xopnds.Length)
        //    {
        //        case 0: // no opnds, must pop or assign
        //            {
        //                if (xStk.Count > 0)
        //                {
        //                    sb2.Append(xStk.Pop() + " ");
        //                    sb2.Append(xoper[0] + " ");
        //                }
        //                else
        //                { // do pop 
        //                    sb2.Append(xoper.Length > 0 ? xoper[0] + " " : "");
        //                }
        ////            }
        ////            break;
        //        case 1: // one opnd and oper
        //            sb2.Append(xopsA[0]);
        //            sb2.Append(xoper[0] + " ");
        //            break;
        //        default: // more than one, push all but 2
        //            for (int i = 0; i < xopnds.Length - 2; i++)
        //                xStk.Push(xopnds[i]);
        //            sb2.Append(xopnds[xopnds.Length - 2] + " " + xopnds[xopnds.Length - 1] + " ");
        //            sb2.Append(xoper[0] + " ");
        //        break;
        //} // end switch opnds.Length
        //  A = B + (C * D) - (E + F) * G;  C D * B + E F + G * - A =

        //while (sb2.Length > 0)
        //{
        // C D * B + F + G * F - E = 
        // 4 3 * 2 + 4 - y =  
        // 3 2 * 5 4 * + x =
        //cprec(ref sx);
        //xopsA = sb2.ToString().Split(expOps, 2);  //, StringSplitOptions.RemoveEmptyEntries);
        //sb2.Remove(0, xopsA[0].Length);
        ////sx.oper = sb2.ToString().Substring(xopsA[0].Length, sb2.Length > xopsA[0].Length + 1 ? 2 : 1).Trim();
        //xopsB = xopsA[0].Split(wsp, StringSplitOptions.RemoveEmptyEntries);
        //if (xopsA.Length > 1)
        //{
        //    xopsA = sb2.ToString().Split(wsp, 2);
        //    sx.oper = xopsA[0];
        //    cprec(ref sx);
        //}

        // ftch op2@, op1@, ocw, nxt@; ftch op1,op2,load ocwReg, nxt@; wrt TOS, fth ocw, op2@, rd nxtOp; 
        //                             
        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        // cwCt = 0	          mem[cwCt.qa]                            mem[cwCt.qa]
        //					                      mem[cwCt.qa + 1]                        mem[cwCt.qa + 1]           
        //										  va[sCw.qb]          vA[op1@, stkCt.qa]
        //										  vb[oCw.qb]          vB[oCw.qb]
        // cwCtA = 2		  cwCtA = nxtScw                                                                    cwCtA = nxtScw


        //int tokenIx = 0;
        //string[] xA;
        //string[] sdelimsB = new string[] { ")", "{", " " };  //  "=", ";", "(", ")", "{", "}", " "
        //xA = sb2.ToString().Split(sdelimsB, StringSplitOptions.RemoveEmptyEntries);
        //                mem.Add(get_vbl(xA[x - 2]) << 16 | get_vbl(xA[x - 1]));
        //                mem.Add(sx.opcw << 16 | (sx.oper == "=" ? get_vbl(xA[0]) : 0));

        //                break;
        //            default:
        //                break;
        //        }
        //        vcnt = 0;
        //    }
        //}
        //mem[mem.Count - 1] |= (int)ops.eop << 16;
        //      return;
        //tokenIx = sb2.ToString().IndexOfAny(expOps);
        //token = tokenIx < 0 ? sb2.ToString() : sb2[tokenIx].ToString();
        //xopsA = sb2.ToString().Substring(0, tokenIx).Split(wsp, StringSplitOptions.RemoveEmptyEntries);
        //if (xopsA.Length < 2)pec
        //{
        //    MessageBox.Show("exted two opnds");
        //    return;
        //}

        //if (token == "=")
        //{
        //    Scws.Add((uint)(get_vbl(xopsA[1], cvblsA) << 16 | Scws.Count)); //get_vbl(xopsA[1], cvblsA)
        //    Scws.Add((uint)((int)(ucdEnum.pway | ucdEnum.eop) << 16 | get_vbl(xopsA[0], cvblsA)));
        //    cwDcd.Add(String.Format("{0}" + " ALU to " + xopsA[0], (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0)));
        //    Console.WriteLine(String.Format("{0}" + " ALU to " + xopsA[0], (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0)));
        //    //sb2.Remove(0, tokenIx + 1);
        //    return;
        //}
        //else
        //{
        //    Scws.Add((uint)(get_vbl(xopsA[0], cvblsA) << 16 | get_vbl(xopsA[1], cvblsA))); //  Xcws.Count)
        //    cwDcd.Add("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
        //    Console.WriteLine("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
        //}
        //sb2.Remove(0, tokenIx + 1);
        //do
        //{
        //    sx.oper = token;
        //    cprec(ref sx);
        //    tokenIx = sb2.ToString().IndexOfAny(expOps);
        //    if (tokenIx < 0)
        //    {
        //        if (sb2.ToString().TrimStart().Length == 0)
        //        {
        //            Scws.Add((uint)(sx.opcw | (int)ucdEnum.eop) << 16);
        //            cwDcd.Add(String.Format("{0} " + "{1}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            Console.WriteLine(String.Format("{0} " + "{1}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            return;
        //        }
        //    }
        //    token = sb2[tokenIx].ToString();
        //    xopsA = sb2.ToString().Substring(0, tokenIx >= 0 ? tokenIx : sb2.Length).Split(wsp, StringSplitOptions.RemoveEmptyEntries);
        //    switch (xopsA.Length)
        //    {
        //        case 0: // back to back opers get opnds from stack
        //            Scws.Add((uint)sx.opcw << 16);
        //            cwDcd.Add(String.Format("{0} " + "{1}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            Console.WriteLine(String.Format("{0} " + "{1}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            Scws.Add((int)(ucdEnum.pop | ucdEnum.etime) << 16);
        //            cwDcd.Add(String.Format("{0}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0)));
        //            Console.WriteLine(String.Format("{0}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0)));
        //            break;
        //        case 1: // normal case 1 opnd per oper
        //            if (token == "=")
        //            {
        //                Scws.Add((uint)((sx.opcw | (int)(ucdEnum.pway | ucdEnum.eop)) << 16 | get_vbl(xopsA[0], cvblsA)));
        //                cwDcd.Add(String.Format("{0 }" + " {1} " + xopsA[0], (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //                Console.WriteLine(String.Format("{0 }" + " {1} " + xopsA[0], (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            }
        //            else
        //            {
        //                Scws.Add((uint)(sx.opcw << 16 | get_vbl(xopsA[0], cvblsA)));
        //                cwDcd.Add(String.Format("{0} " + "{1} " + xopsA[0], (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //                Console.WriteLine(String.Format("{0} " + "{1} " + xopsA[0], (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            }
        //            break;
        //        case 2: // push alu, get 2 opnds
        //                // mem[mem.Count - 2] |= ((int)ops.push);
        //            Scws.Add((uint)(sx.opcw | (int)ucdEnum.push) << 16);
        //            cwDcd.Add(String.Format("{0}" + "{1}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            Console.WriteLine(String.Format("{0}" + "{1}", (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x8f0), (ucdEnum)((Scws[Scws.Count - 1] >> 16) & 0x1f)));
        //            Scws.Add((uint)(get_vbl(xopsA[0], cvblsA) << 16 | get_vbl(xopsA[1], cvblsA)));
        //            cwDcd.Add("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
        //            Console.WriteLine("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
        //            break;
        //        default: // probably an error, expr could not have more than 2 ?????????
        //            break;
        //    }
        //    sb2.Remove(0, tokenIx + 1);
        //}
        //while (sb2.ToString().Trim().Length > 0);

        //return;
        //    }
        //}
        //}


        public class cFunc
        {
            public string name; public int vval; public string type;
            public int scn_ix; public int vix = unchecked((int)-1);
            public List<string> args = new List<string>();
            public List<string> locals = new List<string>();
            public List<uint> cwds = new List<uint>();
            public int cwix;
            public int mcnt;
            public int vcnt;
            public List<int> calls = new List<int>();
            public StringBuilder sb = new StringBuilder();
        }

        public List<cFunc> cfuns = new List<cFunc>();


        public void begin(String fPath, ListBox lB1in)  //  , CEngine pE
        {
            lB1 = lB1in;
            lineno = 0;
            string demostr = fPath;
            StreamReader sR = new StreamReader(@"C:\Users\Karl\ASTDemo.cs");
            //StreamReader sR = new StreamReader(fPath);
            string demo = fPath;
            //StreamReader sR = new StreamReader(demostr.ToString());
            StringBuilder sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
                                                                  //sB = new StringBuilder("main() { y = 2 + 3 * 4 - 4; }");
            CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
            node = tree.GetRoot();
            //cEWalker = new CEngineWalker();
            //cEwalker. = cV;
            cEWalker.parser = this;
            cEWalker.uCwds.Add(0);   //  C statements use or to set enums
                                     //cEWalker.uCwds = uCwds;
            cEWalker.Visit(node);
        }


        [Flags]
        public enum UcdEnum
        {
            //      
            call = 0x30000000,  // 
            rtn = 0x40000000,  // 
            tna = 0x20000000,  // 0x000E combines with cond opers and uses spare opcodes
            fna = 0x10000000,  // 0x000F
            eop = 0x08000000,  //
            gtr = 0x04100000,
            eql = 0x02100000,  // !eql = 0x0510
            less = 0x01100000,
            pway = 0x00800000,  // 
            push = 0x00500000,  // may use 0x8040 to stack call parms, or with ALU codes 
            pop = 0x00300000,  // 
            etime = 0x00100000,  // part of opcode for ALU and compares. decoded also for sequencing
            nop = 0x00000000, bnd = 0x00110000, bxo = 0x00120000, bor = 0x00130000,
            add = 0x00140000, sub = 0x00150000, mpy = 0x00160000, dvd = 0x00170000,
            mlo = 0x00180000, lsf = 0x00190000, rsf = 0x001A0000, ldm = 0x000E0000,
            stm = 0x000F0000
        }

        [Flags]
        public enum RVenum
        {
            LUI = 0x00000037,
            BEQ = 0x00000063,
            BNE = 0x00001063,
            BL = 0x00004063,
            BGE = 0x00005063,
            BLTU = 0x00006063,
            BGEU = 0x00007063,
            ADD = 0x00000033,
            SUB = 0x00000033,
            XOR = 0x00004033,
            OR = 0x00006033,
            AND = 0x00007033
        }


        public class CStmnt
        {
            public int cwix, srcln, loopix, endix;
            public string name, xprn, kwd, cond;
            SyntaxNode node;
            public List<SyntaxNodeOrToken> ns;
            public CStmnt(int lno, SyntaxNode k)
            {
                srcln = lno;
                node = k;
                ns = new List<SyntaxNodeOrToken>(k.ChildNodesAndTokens());

            }
            public List<SyntaxNodeOrToken> nTs
            { get { return ns; } }
            public class CBlk
            {
                CBlk(CStmnt p)
                {
                    parent = p;
                }
                CStmnt parent;
                //SyntaxNode Node;
                bool cMet;
                public List<SyntaxNodeOrToken> ns;
                public CBlk(CStmnt p, SyntaxNode n)
                {
                    parent = p;
                    ns = new List<SyntaxNodeOrToken>(n.ChildNodesAndTokens());
                }
                public String ckwd { get { return parent.kwd; } }
                public bool cmet { get { return cMet; } }
                public String lineno { get { return parent.srcln.ToString(); } }
            }
            public class CIf
            {
                CIf(CStmnt p)
                {
                    parent = p;
                }
                CStmnt parent;
                public CIf(CStmnt p, SyntaxNode n)
                { parent = p; }
                //public LinkedList<CStmnt> tList = new LinkedList<CStmnt>();
                //public LinkedList<CStmnt> fList = new LinkedList<CStmnt>();
                //public int condix;
                public int tcx;
                public int fcx;
                public int enx;
                public String ckwd { get { return parent.kwd; } }
                public String lineno { get { return parent.srcln.ToString(); } }
            }
            public class CFor
            {
                CStmnt parent;
                public CFor(CStmnt p, SyntaxNode n)
                { parent = p; }
                public List<CStmnt> preAsgn = new List<CStmnt>();
                public List<CStmnt> postAsgn = new List<CStmnt>();
                public string init, post;
                public string cond { get { return parent.cond; } }
                //public LinkedList<CStmnt> fList = new LinkedList<CStmnt>();
                public CStmnt initref;
                public CStmnt tref;
                public CStmnt endref;
                public int condix;
                public int bodyix, myLoopix;
                public int loopix { get { return myLoopix; } set { myLoopix = value; } }
                public int tcx;
                public int fcx;
                public String ckwd { get { return parent.kwd; } }
                public String lineno { get { return parent.srcln.ToString(); } }
            }
            public class CWhile
            {
                CStmnt parent;
                SyntaxNode Node;
                public CWhile(CStmnt p, SyntaxNode n)
                {
                    parent = p;
                    Node = n;
                }

                public SyntaxNode node { get { return Node; } }
                public string kwd { get { return parent.ns[0].AsToken().ValueText; } }
                public SyntaxNode cond { get { return parent.ns[2].AsNode(); } }
                public SyntaxNode xprn { get { return parent.ns[4].AsNode(); } }
                public int lineno { get { return parent.srcln; } }

                //public int tIx;
                public int fIx;
                public int loopix;
            }
            public class CDoWhile
            {
                CStmnt parent;
                SyntaxNode Node;
                public CDoWhile(CStmnt p, SyntaxNode n)
                {
                    parent = p;
                    Node = n;
                }
                public string ckwd { get { return "dowhile"; } }
                public string cond { get { return (Node).ToString(); } }
                public string xprn { get { return (Node).ToString(); } }
                public int lineno { get { return parent.srcln; } set { parent.srcln = value; } }
                public int loopix;
                public int condix, bodyix, endix;
            }
            public class CSwitch
            {
                CStmnt parent;
                public CSwitch(CStmnt p, SyntaxNode n)
                { this.parent = p; }
                public List<CStmnt> preAsgn = new List<CStmnt>();
                public List<CStmnt> postAsgn = new List<CStmnt>();
                public string cond;
                public String ckwd { get { return parent.kwd; } }
                public int lineno { get { return parent.srcln; } }

            }


            public Stack<VN> vNStk = new Stack<VN>();
        }
    }
