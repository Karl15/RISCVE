using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RISCVE
{
    public class RISCV
    {
        ListBox Listbox1;
        int[] ImmHi = new int[64];
        int[] ImmLo = new int[64];
        uint[] Regs = new uint[32];
        

        List<char> Mem = new List<char>();
        StringBuilder sB;


        uint[] nMsk = new uint[33]  {0x0000, 0x0001, 0x0003, 0x0007, 0x000f,
                                               0x001f, 0x003f, 0x007f, 0x00ff,
                                               0x01ff, 0x03ff, 0x07ff, 0x0fff,
                                               0x1fff, 0x3fff, 0x7fff, 0xffff,
                                               0x0001ffff, 0x0003ffff, 0x0007ffff, 0x000fffff,
                                               0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
                                               0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff,
                                               0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};


        public enum opCodes
        {
            LUI = 0x37, AUIPC = 0X17, JAL = 0X6F,
            JALR = 0X67,
            BEQ = 0X00000063, BNE = 0X00004063, BLT = 0X00001063,
            BGE = 0X00005063, BLTU = 0X00006063, BGEU = 0X00007063,
            LB = 0X00000003, LH = 0X00001003, LW = 0X00002003,
            LBU = 0X00004003, LHU = 0X00005003,
            POP = 0X00003003, PEEK = 0X00006003, GSC = 0X00007003,
            SB = 0X00000023, SH = 0X00001023, SW = 0X00002023,
            PUSH = 0X000003023, CSC = 0X00007003,
            ADDI = 0X00000013, SLTI = 0X00002013, SLTIU = 0X00003013,
            XORI = 0X00004013, ORI = 0X00006013, ANDI = 0X00007013,
            SLLI = 0X00001013, SRLI = 0X0005013, SRAI = 0X00005013,
            ADD = 0X00000033, SUB = 0X40000033, SLL = 0X00001033,
            SLT = 0X00002033, SLTU = 0X00003033, XOR = 0X00004033,
            SRL = 0X00005033, SRA = 0X40005033, OR = 0X00006033,
            AND = 0X00007033,
            FENCE = 0X0000000F, ECALL = 0X00000073, EBREAK = 0X00100073
        }
        public enum  BFun3
        {
            BEQ = 0x0000, BNE = 0x1000, BLT = 0x4000, BGE = 0x5000, BLTU = 0x6000, BGEU = 0x7000
        }

        public enum LFun3
        {
            LB = 0x0000, LH = 0x1000, LW = 0x2000, LBU = 0x4000, LHU = 0x5000
        }

        public enum IFun3
        {
            ADDI = 0x0000, SLTI = 0x2000, SLTIU = 0x3000, XORI = 0x4000, ORI = 0x6000, ANDI = 0x7000,
            LB = 0x0000, LH = 0x10000,  LW = 0x20000
        }

        public enum RFun3
        {
            ADD = 0x0000, SUB = 0x0000, SLL = 0x1000, SLT = 0x2000, 
            XOR = 0x4000, SRL = 0x5000, SRA = 0x5000, OR = 0x6000, AND = 0x7000,
        }

        //public enum FENCE
        //{
        //    FENCE = 0x0000
        //}

        //public enum ECALL
        //{
        //    ECALL = 0x0000
        //}


        public List<int> rs1fld = new List<int>();
        public List<int> rs2fld = new List<int>();
        public List<int> rdfld = new List<int>();
        public List<int> fun3fld = new List<int>();
        public List<int> vstack = new List<int>();
        public Stack<int> stack = new Stack<int>();


        int Iar = 0, Mar = 0;
        uint IR = 0;
        uint Imm20, Imm12, PC;
        uint OpCode;
        uint fun3, fun7;
        int rd;
        int rs1 = 0x1;
        int rs2 = 0x2;
        int stkCt = 0, TOS, NOS, stkPtr = 0;
        int sAdr, bAdr;
        bool baseInst, immNeg;
        string type, oper;
        List<uint> operL;
        public void push(int val)
        {
            if (stkCt < 2)
            { 
                if (stkCt == 0)
                    Regs[0x1e] = (uint)val;
                    else
                    Regs[0x1f] = (uint)val;
            }
                else
                     stack.Push(val);
            //Regs[14] = 11;
            //Regs[15] = 12;
            //Regs[14] = Regs[14] + Regs[15];
        }

        public void pop()
        {
            TOS = (int)Regs[rs1];
            Regs[rs1] = Regs[rs2];
            Regs[rs2] = (uint)stack.Pop();
        }


        public int peek()
        {
            return stkCt > 2 ? (int)(stack.Peek()) : stkCt == 1 ? (int)Regs[0x1e] : (int)Regs[0x1f];
        }
        public void Build(string fPath, ListBox lB1in)
        {
            ListBox lB1 = lB1in;
            int lineno = 0;

            rdfld.Add(0);
            rs1fld.Add(0);
            rs2fld.Add(0);
            for (int i = 1; i < 16; i++)
            {
                rdfld.Add(i * 0x80);
                rs1fld.Add(i * 0x8000);
                rs2fld.Add(i * 0x100000);
            }
            for (int i = 1; i < 8; i++)
            {
                fun3fld.Add(i * 0x1000);
            }

            Console.WriteLine("rd vals");
            foreach (int rd in rdfld)
                Console.WriteLine(rd >> 7);
            Console.WriteLine("r1 vals");
            foreach (int rd in rs1fld)
                Console.WriteLine(rd >> 15);
            Console.WriteLine("rd vals");
            foreach (int rd in rs2fld)
                Console.WriteLine(rd >> 20);

            //string[] opNms = Enum.GetNames(typeof(opCodes));
            List<string> opL = new List<string>(Enum.GetNames(typeof(opCodes)));
            //List<string> opNmL = opNms.ToList<string>();
            //StreamReader sR = new StreamReader(@"C:\Windows.old.001\Users\Karl\Dropbox\ASTEngine\ASTDemo.cs");
            //StringBuilder sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
            sB = new StringBuilder("main() { y = 2 + 3 + 4 - 5; x = y - 6; }");
            //Mem.Add('y');
            //Mem.Add('2');
            //Mem.Add('3');
            CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
            SyntaxNode node = tree.GetRoot();
            CEngineWalker cEwalker = new CEngineWalker();
            //cEwalker.parser = this;
            //cEwalker.Visit(node);
            //decode();
            List<string> operL = new List<string>(); 
            string[] opNames = Enum.GetNames(typeof(opCodes));
            operL = opNames.ToList();
            //nodeStk.Pop();

            //foreach (string opCode in operL)
            //{
            //opCodes opCodes = (opCodes)Enum.Parse(typeof(opCodes), opCode);
            //Console.WriteLine("   {0} ({0:X})", opCode);
            //var values = Enum.GetValues(typeof(opCodes));
            //Console.WriteLine(opCode);
            //}
            cEwalker.Visit(node);
        }


        public void decode()
        {
            //IR = 0x00202D01;
            sB = new StringBuilder();
            //string opS = operL.Find()
            IR = (uint)opCodes.ADDI;

            //OpCode = IR & 0x7f;
            switch(IR)
            {
                case 0x33: // R-Type
                    switch(fun3)
                    {
                        case 0: // ADD/SUB
                            break;
                        case 1: // SLL
                            break;
                        case 2: // SLT
                            break;
                        case 3: // SLTU
                            break;
                        case 4: // XOR
                            break;
                        case 5: // SRL/SRA
                            break;
                        case 6: // OR
                            break;
                        case 7: // AND
                            break;
                    }
                    break;
                case 0x13: // I-Type
                    break;
                case 0x23: // S-Type
                    break;
                case 0x63: // B-Type
                    break;
                case 0x17: // U-Type
                case 0x37:
                    break;
                case 0x67: // J-Type
                case 0x6f:
                    break;
            }
            Imm20 = IR & 0xfffff000;    // U-Type
            rd = (int)(IR >>= 7);
            rs1 = rd >>= 8;
            rd &= 0x1f;
            rs2 = rs1 >>= 5;
            rs1 &= 0x1f;
            Imm12 = (uint)rs2;
            rs2 &= 0x1f;
            //IR >>= 5;   // shift out rs2 field
            fun3 = Imm20 & 0x00007000;
            fun7 = Imm20 & 0xf7000000;

            switch (OpCode) //  Types for fun3 and fun7 decode
            {   //  rd and Imm12[[4:7] previously assigned 
                case 0x13:
                    type = "Itype";
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
                    switch (fun3)
                    {
                        case 0x0:    // add/sub
                            Regs[rd] = (IR & 0x80000000) == 0 ? Regs[rs1] + Regs[rs2] : Regs[rs1] - Regs[rs2];
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
                case 0x03:
                    type = "Itype";
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
                case 0x23:
                    type = "Stype";
                    switch (fun3)    // fun3 decode
                    {
                        case 0x0:
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
                    }
                    break;
                case 0x63:
                    type = "Btype";
                    //rs1 = (int)(IR & 0x1f);
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
                    switch (OpCode)
                    {   // need Type for U-Type and J-Type
                        case 0x37:
                            type = "Utype";     // lui
                            Regs[rd] = Imm20;
                            break;
                        case 0x17:
                            type = "Utype";     // auipc
                            Regs[rd] = PC + Imm20;
                            break;
                        case 0x6f:
                            type = "Jtype";     // jal
                            Regs[rd] = PC + 4;
                            PC = Imm20;
                            break;
                        case 0x67:
                            type = "Jtype";     // jalr
                            Regs[rd] = PC + 4;
                            PC = Imm20;
                            break;
                        default:
                            break;
                    }

            }
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

        }
        public void Run()
        {
            IR = 0x06900003;
            if ((IR & 0x00000003) == 3)
                    baseInst = true;
            if ((IR & 0x80000000) == 0x80000000)
                immNeg = true;
            IR = Mem[Iar];
            if (baseInst)
            {
                switch (IR & 0x00000007F)
                {
                    case 0x00000003:
                        type = "I_Type";
                        switch(IR & 0x00000000)
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
                        switch(IR & 0x00000023)
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
                        switch(IR & 0x00070033)
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

        public class CStmnt
        {
            public int cwix, srcln, loopix;
            public string name, xprn, kwd, cond;
            public StringBuilder expSB;

            public CStmnt()
            { }
            public CStmnt(int lno, string k)
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
                public string ckwd { get { return parent.kwd; } }
                public string lineno { get { return parent.srcln.ToString(); } }
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
                public string ckwd { get { return parent.kwd; } }
                public string lineno { get { return parent.srcln.ToString(); } }
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
                public string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
                public string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
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
                public string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
                public string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
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
                public string ckwd { get { return parent.kwd; } }
                public int lineno { get { return parent.srcln; } }

            }
        }

    }   //RISCV

    public class CEngineWalker : CSharpSyntaxWalker
    {
        //NOTE: Make sure you invoke the base constructor with 
        //the correct SyntaxWalkerDepth. Otherwise VisitToken()
        //will never get run.
        //public CEngineWalker() : base(SyntaxWalkerDepth.Token)
        //{
        //}
        public class Cval
        {
            public Cval(string snm, List<Cval> cvalsA)
            {
                name = snm;
                vix = cvalsA.Count;
                Cvals = cvalsA;
            }
            public int val
            {
                get { return vval; }
                set { vval = value; }
            }
            public List<Cval> Cvals;
            public string name; public int vval; public string type;
            public int scn_ix; public int vix = -1;
        }
        static int Tabs = 0;
        RISCV rSCV = new RISCV();
        public string indents;
        public Stack<SyntaxNode> nodeStk = new Stack<SyntaxNode>();
        public Stack<Cval> cVstack = new Stack<Cval>();
        public Stack<int> cValstack = new Stack<int>();
        public List<Cval> cVals = new List<Cval>();
        public SyntaxNode node;
        public SyntaxToken token;
        public List<uint> uCwds = new List<uint>();
        public uint uCwd;
        public int cwIx = 0;
        public int TOS;
        //public StringBuilder rpn = new StringBuilder();

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

        public override void Visit(SyntaxNode node)
        {
            //Console.WriteLine("Visit " + node.Kind().ToString());

            var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
            var linenum = lineSpan.StartLinePosition.Line;
            Cval cV;
            int op1 = 0, op2 = 0;
            int lineno = linenum + 1;
            Tabs++;
            indents = new string('\t', Tabs);
            switch (node.Kind().ToString())
            {
                case "ExpressionStatement":
                    nodeStk.Push(node);
                    Console.WriteLine(" visit " + nodeStk.Peek().ToString());
                    break;
                case "SimpleAssignmentExpression":
                    nodeStk.Push(node);
                    //Console.WriteLine("  visit " + nodeStk.Peek().Kind().ToString());
                    break;
                case "NumericLiteralExpression":
                    nodeStk.Push(node);
                    //Console.WriteLine("  visit " + nodeStk.Peek().Kind().ToString());
                    break;
                case "IdentifierName":
                    nodeStk.Push(node);
                    //Console.WriteLine("  visit " + nodeStk.Peek().Kind().ToString());
                    break;
                case "AddExpression":
                case "SubtractExpression":
                case "BitwiseAndExpression":
                case "BitwiseOrExpression":
                case "ExclusiveOrExpression":
                case "MultiplyExpression":
                case "DivideExpression":
                case "EqualsExpression":
                case "NotEqualsExpression":
                case "GreaterThanExpression":
                case "LessThanExpression":
                case "LessThanOrEqualExpression":
                case "GreaterThanOrEqualExpression":
                    //Console.WriteLine(" base visit " + nodeStk.Peek().Kind().ToString());
                    break;
                case "CompilationUnit":
                case "ParameterList":
                    break;
                //case "ForStatement":
                //    //Console.WriteLine("push " + nodeStk.Peek().Kind().ToString());
                //    //Console.WriteLine(" For " + node.ToString());
                //    CStmnt cS = new CStmnt();
                //    CStmnt.CFor cFor = new CStmnt.CFor(cS, node);
                //    break;
                //case "WhileStatement":
                //    //Console.WriteLine("push " + node.ToString());
                //    Console.WriteLine(" While " + node.Kind().ToString());
                //    cS = new CStmnt(linenum, "while");
                //    CStmnt.CWhile cWhile = new CStmnt.CWhile(cS, node);
                //    var whileCond = ((WhileStatementSyntax)node).Condition;
                //    Visit(whileCond);
                //    //Visit(node);
                //    //parser.cKwds(node, null, parser.cvbls, parser.uCwds);
                //    //parser.makCond(node.Condition., parser.cvbls, uCwds); // blds opwds for cond eval
                //    //rplSB(cWhile.cond);
                //    //cWhile.cond = rpb.ToString();
                //    //makCond(cWhile.cond, parser.cvbls, uCwds); // blds opwds for cond eval
                //    cWhile.loopix = uCwds.Count;    // - 1;
                //                                    //    Scws[Scws.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime); // fcond jumps over body
                //                                    //    cWhile.bodyix = Scws.Count;
                //                                    //    delimIx = nxtDelim();
                //                                    //    nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
                //                                    //    Scws.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cWhile.bodyix)); // fcond jumps over body
                //                                    //    Scws[cWhile.loopix] |= (uint)Scws.Count; ;
                //    break;
                //case "DoWhileStatement":
                //    Console.WriteLine(" DoWhile " + node.Kind().ToString());
                //    //Console.WriteLine("Visit " + node.ToString());
                //    cS = new CStmnt();
                //    CStmnt.CDoWhile cDoWhile = new CStmnt.CDoWhile(cS, node);
                //    break;
                //case "SwitchStatement":
                //    Console.WriteLine(" Switch " + node.Kind().ToString());
                //    //Console.WriteLine("Visit " + node.ToString());
                //    //parser.cKwds(node, null, parser.cvbls, parser.uCwds);
                //    cS = new CStmnt();
                //    CStmnt.CSwitch cSwitch = new CStmnt.CSwitch(cS, node);
                //    break;
                case "ParenthesizedExpression":
                    //nodeStk.Push(node);
                    //Console.WriteLine(" visit push ParensExpr " + nodeStk.Peek().Kind().ToString());
                    break;
                case "Block":
                    nodeStk.Push(node);
                    //Console.WriteLine(" base visit " + nodeStk.Peek().Kind().ToString());
                    break;
                default:
                    break;
            }
            //Console.WriteLine(" base visit " + node.Kind().ToString());

            base.Visit(node);
            Tabs--;
            switch (node.Kind().ToString())
            {
                case "Block":
                    Console.WriteLine(" base visit " + nodeStk.Pop().Kind().ToString());
                    break;
                case "IdentifierName":
                    Console.WriteLine(" base visit Identifier " + node.ToString());
                    if ((cV = cVals.Find(vnm => vnm.name == node.ToString())) == null)
                    {
                        cV = new Cval(node.ToString(), cVals);
                        cVals.Add(cV);
                        //.memCpu.vRam.vals.Add(0);
                    }
                    //nodeStk.Pop();
                    cVstack.Push(cV);
                    Console.WriteLine(" cVstack Push " + cV.name + " value = ", cV.val);
                    //cValstack.Push(cV.val);
                    break;
                case "NumericLiteralExpression":
                    if ((cV = cVals.Find(vnm => vnm.name == node.ToString())) == null)
                    {
                        cV = new Cval(node.ToString(), cVals);
                        string nS = node.ToString();
                        cV.val = (int.Parse(nS));
                        cVals.Add(cV);
                        //    vRam.vals.Add(cV.val);
                    }
                    cVstack.Push(cV);
                    //cValstack.Push(cV.val);
                    Console.WriteLine(" cVstack push " + node.ToString());
                    //nodeStk.Pop();
                    break;
                case "PredefinedType":
                    //Console.WriteLine("--  " + node.Kind().ToString());
                    break;
                case "ExpressionStatement":
                    Console.WriteLine(" base visit Xpr " + nodeStk.Pop().ToString());
                    break;
                case "SimpleAssignmentExpression":
                    Console.WriteLine("-- Visit " + node.Kind().ToString());
                    var saL = ((AssignmentExpressionSyntax)node).Left;
                    var saR = ((AssignmentExpressionSyntax)node).Right;
                    var eqoper = ((AssignmentExpressionSyntax)node).OperatorToken;
                    if ((cV = cVals.Find(vnm => vnm.name == saL.ToString())) == null)
                        cVals.Add(cV = new Cval(saL.ToString(), cVals));
                    if (saR.Kind().ToString() == "NumericLiteralExpression") //xStk.Count == 0)
                        cV.val = int.Parse(saR.ToString());
                    else
                    {
                        Console.WriteLine(" uCwd opers use TOS, '=' Pops put away  " + cValstack.Peek().ToString());
                        cV.val = cValstack.Pop();    // (int)tknStk.Pop().Value;
                                                //if (saR == xnodeStk.Peek())
                                                //    xnodeStk.Pop();
                    }
                    Console.WriteLine(cV.name + " = " + cV.val.ToString());
                    //cwStk.Push(uCwd);
                    break;
                case "ParenthesizedExpression":
                    //nodeStk.Push(node);
                    //Console.WriteLine("-- Parens Exp " + nodeStk.Peek().Kind().ToString());
                    break;
                case "AddExpression":
                case "SubtractExpression":
                case "BitwiseAndExpression":
                case "BitwiseOrExpression":
                case "ExclusiveOrExpression":
                case "MultiplyExpression":
                case "DivideExpression":
                case "EqualsExpression":
                case "NotEqualsExpression":
                case "GreaterThanExpression":
                case "LessThanExpression":
                case "LessThanOrEqualExpression":
                case "GreaterThanOrEqualExpression":
                    //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
                    //                    sCwA[2]                                eop ? sCwA[cwCt.qb] : 0
                    //                    sCwB[0]              sCwB[cStk.qb + 1] eop ? 0 : sCwB[cStk.qb + 1] 
                    //                                         va[sCwA.qa]       pway ? pway@ 
                    //                                                            : etime ? cStk.qa
                    //                                         vb[pop ? cStk.qa
                    //                                          : etime ? sCwA.qb 
                    //                                           : sCwA.qb]                            
                    //                    cwCt.qa = 2		   cwCtA = cwCt.qa + 1 

                    Console.WriteLine(" base visit BinaryExpr " + node.ToString());
                    var lft = ((BinaryExpressionSyntax)node).Left;
                    var rgt = ((BinaryExpressionSyntax)node).Right;
                    var oper = ((BinaryExpressionSyntax)node).OperatorToken;
                    var tos = cVstack.Peek().val + cVstack.Peek().val;
                    //switch (rgt.Kind().ToString())
                    //{
                    //    case "NumericLiteralExpression":
                    //    case "IdentifierName":
                    //        if ((cV = cVals.Find(vnm => vnm.name == rgt.ToString())) != null)
                    //            op2 = cV.val;
                    //        else { }
                    //        break;
                    //    default:
                    //        Console.WriteLine(" pop op2 = " + cValstack.Peek().ToString());
                    //        op2 = cValstack.Pop();
                    //        break;
                    //}
                    //switch (lft.Kind().ToString())
                    //{
                    //    case "NumericLiteralExpression":
                    //    case "IdentifierName":
                    //        if ((cV = cVals.Find(vnm => vnm.name == lft.ToString())) != null)
                    //            op1 = cV.val;
                    //        break;
                    //    default:
                    //        Console.WriteLine(" pop op1 = " + cValstack.Peek().ToString());
                    //        op1 = cValstack.Pop();
                    //        break;
                    //}
                    Console.WriteLine(" oper = " + oper.ValueText);

                    switch (oper.ValueText)
                    {
                        case "+":
                            op1 = cVstack.Pop().val;
                            op2 = cVstack.Peek().val;
                            //TOS = cVstack.Pop().val + cVstack.Pop().val;

                            cVstack.Peek().name = node.Kind().ToString();
                            cVstack.Peek().val = op1 + op2;
                            cwIx = uCwds.Count;
                            uCwds.Add((uint)UcdEnum.add);
                            break;
                        case "-":
                            op1 = cVstack.Pop().val;
                            op2 = cVstack.Pop().val;
                            //TOS = cVstack.Pop().val - cVstack.Pop().val;
                            cVstack.Peek().val = op1 - op2;
                            cwIx = uCwds.Count;
                            uCwds.Add((uint)UcdEnum.sub);
                            break;
                        case "*":
                            TOS = cVstack.Pop().val + cVstack.Pop().val;
                            cVstack.Peek().val = TOS;
                            cwIx = uCwds.Count;
                            uCwds.Add((uint)UcdEnum.mpy);
                            break;
                        case "/":
                            cValstack.Push(op1 / op2);
                            cwIx = uCwds.Count;
                            uCwds.Add((uint)UcdEnum.dvd);
                            break;
                        case "&":
                            cValstack.Push(op1 & op2);
                            //xnodeStk.Push(node);
                            break;
                        case "|":
                            cValstack.Push(op1 | op2);
                            //xnodeStk.Push(node);
                            break;
                        case "^":
                            cValstack.Push(op1 ^ op2);
                            //xnodeStk.Push(node);
                            break;
                        case "==":
                            cValstack.Push(op1 == op2 ? 0 : 1);
                            //xnodeStk.Push(node);
                            break;
                        case "!=":
                            cValstack.Push(op1 != op2 ? 0 : 1);
                            //xnodeStk.Push(node);
                            break;
                        default:
                            break;
                    }
                    if (cValstack.Count > 0)
                    {
                        Console.WriteLine(" TOS = " + cValstack.Peek());
                    }
                    else
                        Console.WriteLine(" Stack MT! = ");

                    break;
                case "IfStatement":
                //NodeStk.Push(node);
                //Console.WriteLine("-- End   " + NodeStk.Peek().Kind().ToString());
                //break;
                case "ElseClause":
                //NodeStk.Push(node);
                //Console.WriteLine("-- End   " + NodeStk.Peek().Kind().ToString());
                //break;
                case "WhileStatement":
                    //NodeStk.Push(node);
                    Console.WriteLine("-- End   " + node.Kind().ToString());
                    //parser.ckwds(node);
                    break;
                default:
                    Console.WriteLine("  Unhandled Node: " + node.Kind().ToString());
                    break;
            }
        }
    }
}
//public override void VisitToken(SyntaxToken token)  // called by VisitNode
//{
//    var lineSpan = token.SyntaxTree.GetLineSpan(token.Span);
//    var linenum = lineSpan.StartLinePosition.Line;
//    //parser.lineno = linenum + 1;
//    Console.WriteLine(token.Kind().ToString() + "   " + token.ValueText);
//    var indents = new string('\t', Tabs);
//    //tknStk.Push(token);
//    //Cvbl cV = new Cvbl(token.ValueText, parser.cVbls);
//    string tKind = token.Kind().ToString();

//    switch (token.Kind().ToString())
//    {
//        case "NumericLiteralToken":
//            //txtStk.Push(token.ValueText);
//            //if ((cV = parser.cVbls.Find(vnm => vnm.name == token.ValueText)) == null)
//            //{
//            //    cV = new Cvbl(token.ValueText, parser.cVbls);
//            //    cV.val = (Int32)token.Value;
//            //    parser.cVbls.Add(cV);
//            //}
//            break;
//        case "IdentifierToken":
//            //txtStk.Push(token.ValueText);
//            //if ((cV = parser.cVbls.Find(vnm => vnm.name == token.ValueText)) == null)
//            //{
//            //    cV = new Cvbl(token.ValueText, parser.cVbls);
//            //    parser.cVbls.Add(cV);
//            //}
//            break;
//        case "OpenParenToken":
//        case "CloseParenToken":
//            break;
//        case "SemicolonToken":
//            break;
//        case "WhileKeyword":
//            break;
//        default:
//            switch (token.Kind().ToString())
//            {
//                case "+":
//                    break;
//                case "-":
//                    break;
//                case "*":
//                    xStk.Push(int.Parse(tknStk.Pop().ValueText) * int.Parse(tknStk.Pop().ValueText));
//                    break;
//                default:
//                    break;
//            }
//            tknStk.Push(token);
//            break;
//    }
//    { }
//}