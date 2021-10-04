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
        public uint[] Regs = new uint[32];
        int r8, r9;
        
        public List<uint> Mem = new List<uint>();
        StringBuilder sB;

        int Iar = 0, Mar = 0;
        uint IR = 0;
        uint Imm20, Imm12, PC;
        uint OpCode;
        uint fun3;
        int rd;
        int rs1;
        int rs2;
        int sAdr, bAdr;
        bool baseInst, immNeg;
        string type;

        uint[] nMsk = new uint[33]  {0x0000, 0x0001, 0x0003, 0x0007, 0x000f,
                                               0x001f, 0x003f, 0x007f, 0x00ff,
                                               0x01ff, 0x03ff, 0x07ff, 0x0fff,
                                               0x1fff, 0x3fff, 0x7fff, 0xffff,
                                               0x0001ffff, 0x0003ffff, 0x0007ffff, 0x000fffff,
                                               0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
                                               0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff,
                                               0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};

        public void Build(string fPath, ListBox lB1in)
        {
            ListBox lB1 = lB1in;
            int lineno = 0;
            //StreamReader sR = new StreamReader(@"C:\Windows.old.001\Users\Karl\Dropbox\ASTEngine\ASTDemo.cs");
            //StringBuilder sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
            //sB = new StringBuilder("main() { y = 2 + 3 + 4 - 5; x = y - 6; }");
            sB = new StringBuilder("y = 2 + 3 + 4 - 5; x = y - 6;");
            CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
            SyntaxNode node = tree.GetRoot();
            CEngineWalker cEwalker = new CEngineWalker();
            //cEwalker.parser = this;
            cEwalker.Visit(node);
            Console.WriteLine(tree);
            return;
            decode();
        }

        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        // cwCt = 0	          mem[cwCt.qa]                            mem[cwCt.qa]
        //					                      mem[cwCt.qa + 1]                       mem[cwCt.qa + 1]           
        //										  va[sCw.qb]          vA[op1@, stkCt.qa]
        //										  vb[oCw.qb]          vB[oCw.qb]
        // cwCtA = 2		  cwCtA = nxtScw                                                                    cwCtA = nxtScw
        //                    va,vb addr          op1, op2, ocw       TOS, op2, ocw

        public void decode()
        {
            sB = new StringBuilder();
            OpCode = IR & 0x7f;
            IR >>= 7;
            Imm12 = IR & 0x0000001f;
            switch (OpCode)
            {
                case 0x23:
                    type = "Stype";
                    //rd = (int)(IR & 0x1f);
                    //Imm12 = IR & 0x0000001f;
                    IR >>= 5;
                    switch (IR & 0x00000007)
                    {
                        case 0x00000000:
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
            }
        }

        public void LI()
        {

        }

        public void LD()
        {

        }
        public void ST()
        {

        }
        public void ADD()
        {

        }
        public void SUB()
        {

        }
        public void AND()
        {

        }
        public void OR()
        {

        }
        public void XOR()
        {

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
            public Cval(SyntaxNode node, List<Cval> cvalsA)
            {
                name = node.ToString();
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
        public string indents;
        //public Stack<SyntaxToken> tknStk = new Stack<SyntaxToken>();
        //public Stack<SyntaxToken> xTstk = new Stack<SyntaxToken>();
        //public Stack<string> txtStk = new Stack<string>();
        public Stack<SyntaxNode> nodeStk = new Stack<SyntaxNode>();
        //public Stack<SyntaxNode> xnodeStk = new Stack<SyntaxNode>();
        //public List<SyntaxNode> asgnLst = new List<SyntaxNode>();
        public Stack<int> xStk = new Stack<int>();
        public List<Cval> cVals = new List<Cval>();
        public Stack<Cval> cVstk = new Stack<Cval>();
        public SyntaxNode node;
        public SyntaxToken token;
        public List<uint> uCwds = new List<uint>();
        public uint uCwd;
        public int cwIx = 0;
        public Stack<uint> cwStk = new Stack<uint>();
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

        /*
          _|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|_______________|c0|___
           cwCt = 0	          mem[cwCt.qa]                           mem[cwCt.qa]
          					                      mem[cwCt.qa + 1]                      mem[cwCt.qa + 1]           
          										  va[sCw.qb]         vA[op1@, stkCt.qa]
        										  vb[oCw.qb]         vB[oCw.qb]
           cwCtA = 2		  cwCtA = nxtScw                                                               cwCtA = nxtScw
                              va,vb addr          op1, op2, ocw      TOS, op2, ocw

                              ixL,ixR             L,R,oper           R,oper             R,oper             Asgn,ixL,ixR
        */
        public override void Visit(SyntaxNode node)
        {
            //Console.WriteLine("Visit " + node.Kind().ToString());

            var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
            var linenum = lineSpan.StartLinePosition.Line;
            Cval cV = new Cval(node, cVals);
            int opL = 0, opR = 0;
            int lineno = linenum + 1;
            //Tabs++;
            //indents = new string('\t', Tabs);
            switch (node.Kind().ToString())
            {
                case "ExpressionStatement":
                    //Console.WriteLine(" visit " + node.Kind().ToString());
                    //var xL = node.ChildNodesAndTokens();
                    //var xLN = xL[0];
                    //var xLNxpr = xL[0].ChildNodesAndTokens();
                    //base.Visit(xL[0].AsNode());
                    //break;
                case "NumericLiteralExpression":
                case "IdentifierName":
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
                    //nodeStk.Push(node);
                    //Console.WriteLine(" visit " + node.Kind().ToString());
                    //break;
                case "SimpleAssignmentExpression":
                    //nodeStk.Push(node);
                    Console.WriteLine(" visit " + node.Kind().ToString());
                    var xL = node.ChildNodesAndTokens();
                    var xLN = xL[0];
                    var xLNxpr = xL[0].ChildNodesAndTokens();
                    base.Visit(xL[0].AsNode());
                    break;
                    //Console.WriteLine("  visit " + node.Kind().ToString());
                    //break;
                case "CompilationUnit":
                    Console.WriteLine(" visit " + node.Kind().ToString());
                    //nodeStk.Push(node);
                    break;
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
                    //nodeStk.Push(node);
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
                    Console.WriteLine(" Block " + node.Kind().ToString());
                    break;
                case "IdentifierName":
                    if ((cV = cVals.Find(vnm => vnm.name == node.ToString())) == null)
                    {
                        cV = new Cval(node, cVals);
                        cV.vix = cVals.Count;
                        cVals.Add(cV);
                        //.memCpu.vRam.vals.Add(0);
                    }
                    Console.WriteLine(node.Kind().ToString());
                    break;
                case "NumericLiteralExpression":
                    if ((cV = cVals.Find(vnm => vnm.name == node.ToString())) == null)
                    {
                        cV = new Cval(node, cVals);
                        string nS = node.ToString();
                        cV.val = (int.Parse(nS));
                        cVals.Add(cV);
                    }
                    Console.WriteLine(node.Kind().ToString());
                    break;
                case "PredefinedType":
                    //Console.WriteLine("--  " + node.Kind().ToString());
                    break;
                case "ExpressionStatement":
                    Console.WriteLine(" end expression " + node.Kind().ToString());
                    break;
                case "SimpleAssignmentExpression":
                    //Console.WriteLine(" assign " + nodeStk.Pop().ToString() + " = " + cVstk.Peek().val);
                    var saL = ((AssignmentExpressionSyntax)node).Left;
                    //var saR = ((AssignmentExpressionSyntax)node).Right;
                    //var eqoper = ((AssignmentExpressionSyntax)node).OperatorToken;
                    if ((cV = cVals.Find(vnm => vnm.name == saL.ToString())) == null)
                    {
                    }
                        cV.val = cVstk.Pop().val;
                    Console.WriteLine("assign " + cV.name + " = " + cV.val.ToString());
                    break;
                case "ParenthesizedExpression":
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

                    /*  stack 0:15 ->  regs 16:31
                     *  stkCt reg 1; TOS reg 2, 
                     */
                    Console.WriteLine(" binary expr " + node.Kind().ToString());
                    var lft = ((BinaryExpressionSyntax)node).Left;  //  num, RI;  else RR
                    var rgt = ((BinaryExpressionSyntax)node).Right; //  num use RI else use RR, name LOAD, binexp TOS
                    var oper = ((BinaryExpressionSyntax)node).OperatorToken;
                    switch (rgt.Kind().ToString())
                    {
                        case "NumericLiteralExpression":
                            //break;
                        case "IdentifierName":
                            if ((cV = cVals.Find(vnm => vnm.name == rgt.ToString())) != null)
                            {
                                opR = cV.val;
                                Console.WriteLine(" opR = " + cV.val.ToString());
                            }
                            break;
                        default:
                            opR = cVstk.Pop().val;
                            Console.WriteLine(" pop opR = " + opR.ToString());
                            break;
                    }
                    switch (lft.Kind().ToString())
                    {
                        case "NumericLiteralExpression":
                            //break;
                        case "IdentifierName":
                            if ((cV = cVals.Find(vnm => vnm.name == lft.ToString())) != null)
                            {
                                opL = cV.val;
                                Console.WriteLine(" opL = " + cV.val.ToString());
                            }
                            break;
                        default:
                            opL = cVstk.Pop().val;
                            Console.WriteLine(" opL = " + opL.ToString());
                            break;
                    }
                    //cwop1 = uCwds[cwIx];
                    //cwop1 |= 0xffff;
                    Console.WriteLine(" oper = " + oper.ToString());

                    switch (oper.ValueText)
                    {
                        case "+":
                            cV.val = opL + opR;
                            break;
                        case "-":
                            cV.val = opL - opR;
                            cwIx = uCwds.Count;
                            //uCwds.Add((uint)UcdEnum.sub);
                            break;
                        case "*":
                            cV.val = opL * opR;
                            break;
                        case "/":
                            cV.val = opL / opR;
                            break;
                        case "&":
                            cV.val = opL & opR;
                            break;
                        case "|":
                            cV.val = opL | opR; ;
                            break;
                        case "^":
                            cV.val = opL ^ opR;
                            break;
                        case "==":

                            break;
                        case "!=":

                            break;
                        default:
                            break;
                    }
                    cV.name = node.ToString();
                    cVstk.Push(cV);
                    cwIx = uCwds.Count; ;
                    Console.WriteLine(" " + cVstk.Peek().name + " = "  + cVstk.Peek().val);

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
                case "CompilationUnit":
                    Console.WriteLine(" End " + node.Kind().ToString());
                    var stree = node.SyntaxTree;
                    return;
                    break;
                default:
                    Console.WriteLine("  Unhandled Node: " + node.Kind().ToString());
                    //Console.WriteLine(" base visit " + nodeStk.Pop().Kind().ToString());
                    break;
            }
        }

    }
}