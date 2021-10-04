using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RISCVE
{
    public class Parser
    {
        //public List<uint> uCwds = new List<uint>();
        //List<uint> Xcws = new List<uint>();
        //public static char[] xOpers = new char[] { '+', '-', '!', '~', '=', '?', ':', '&', '|', '^', '*', '/', '<', '>', '%' };  // 71, 94 , 364
        //char[] cBs = new char[] { '{', '}' };
        //char[] wsp = new char[] { };
        //public char[] opers = new char[] { '+', '-', '*', '/', '%', '&', '|', '^' };  // '<', '>',, '<', '>' };
        //public char[] expOps = new char[] { ';', '=', '+', '-', '!', '~', '(', ')', '?', ':', '&', '|', '^', '*', '/', '<', '>', '%' };
        //string[] inSplitA;
        //string token = " ";
        //public int lineno;
        //public int cbcnt = 0;
        //public ListBox lB1;
        ////public ListBox lB2;
        ////public ListBox lB3;
        //public List<VNode> vNs = new List<VNode>();
        //public CStmnt cStmnt;

        //public string seq = "";
        //public bool reset;
        //public StringBuilder sB;
        //public StringBuilder rpb = new StringBuilder();
        public List<string> sBT = new List<string>();
        public Stack<string> sStk = new Stack<string>();
        List<uint> cList = new List<uint>();
        public LinkedList<CStmnt> sXList = new LinkedList<CStmnt>();
        public LinkedListNode<CStmnt> llN;
        List<String> xS = new List<string>();
        public List<string> sxl = new List<string>();
        char[] delims = new char[] { '=', '(', ')', '{', '}', ';' };
        char[] rparenDelims = new char[] { '=', ')', '{', '}', ';' };
        char[] semic = new char[] { ';' };
        char[] rparen = new char[] { ')' };
        string[] sdelims = new string[] { "(", ")", "{", "}", "=", " " };
        List<String> cwDcd = new List<string>();
        int delimIx = 0;
        SyntaxToken tkn;
        SyntaxNode node;

        // Lambda expression as executable code.
        //string str = "2+3*4-5"
        static Func<int, bool> deleg = i => i < 5;
        bool delegRslt = deleg(4);
        //Console.WriteLine("deleg(4) = {0}", deleg(4));
        //string[] sA = str.Split(xOpers, StringSplitOptions.RemoveEmptyEntries);

        static Func<int, int, int, int, int> delInt = (i, j, k, l) => i + j * k - l;
        static int rslt = delInt(2, 3, 4, 5);

        static Func<int> delInt1 = () => 2 + 3 * 4 - 5;
        static int rslt1 = delInt1();

        static Func<int> delInt2 = () => 2 + 3 * 4 - 5 + 10;
        static int rslt2 = delInt2();

        static Func<int, int, int> addDel = (j, i) => i + j;
        static int addX = addDel(2, 3);

        static Func<int, int, int> subDel = (j, i) => i - j;
        static int subx = subDel(3, 2);

        static Func<int, int, int> mulDel = (j, i) => i * j;
        static int mulX = mulDel(2, 3);

        static Func<int, int, int> dvdDel = (j, i) => i / j;
        static int dvdX = dvdDel(2, 3);

        static Func<int, int, int> andXpr = (j, i) => i & j;
        static int andx = andXpr(3, 2);

        static Func<int, int, int> orXpr = (j, i) => i | j;
        static int orX = orXpr(2, 3);

        static Func<int, int, int> xorDel = (j, i) => i ^ j;
        static int xorX = xorDel(2, 3);



        // Invoke the delegate and display the output.
        //static string sValIn;
        //static Func<string, int> sVal = s => Int32.Parse(s[0].ToString()) + Int32.Parse(s[1].ToString()) * Int32.Parse(s[2].ToString());
        //string sxpr = "2+3*7";
        static string[] sIn = "2+3*7-8".Split(new char[] { '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
        static List<int> nums = new List<int>();
        //foreach(string s in sIn)
        static Func<string[], int> sVala = s => Int32.Parse(s[0]) + Int32.Parse(s[1]) * Int32.Parse(s[2]) - Int32.Parse(s[3]);
        int sValRslt = sVala(sIn);
        int zz = -1;


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
            public Stack<SyntaxNode> nodeStk = new Stack<SyntaxNode>();
            public MemCpu.VNode RVN;
            public Stack<MemCpu.VNode> VNStk = new Stack<MemCpu.VNode>();
            public Stack<Int32> xStk = new Stack<Int32>();
            public SyntaxNode node;
            //public XNode
            public Parser parser;
            public List<MemCpu.VNode> vNs = new List<MemCpu.VNode>();
            public List<uint> uCwds;

            //public delegate void Del(string message);
            // Create a method for a delegate.
            //public static void DelegateMethod(string message)
            //{
            //    Console.WriteLine(message);
            //}
            // Instantiate the delegate.
            //Del handler = DelegateMethod;
            // Call the delegate.
            //handler("Hello World")

            public int Func(string str, int x, int y, int z)
            {
                return str == "+" ? x + y
                     : str == "-" ? x - y
             : 0;
            }

            public delegate int VDel(string oper, MemCpu.VNode vL, MemCpu.VNode vR);

            public static int Add(string oper, MemCpu.VNode vL, MemCpu.VNode vR)
            {
                return ((int)vL.val + (int)vR.val);
            }
            public static int Sub(string oper, MemCpu.VNode vL, MemCpu.VNode vR)
            {
                return ((int)vL.val - (int)vR.val);
            }
            public static int Mpy(string oper, MemCpu.VNode vL, MemCpu.VNode vR)
            {
                return ((int)vL.val * (int)vR.val);
            }
            public static int Dvd(string oper, MemCpu.VNode vL, MemCpu.VNode vR)
            {
                return ((int)vL.val / (int)vR.val);
            }





            //vhandler = Add + Sub, + Mpy + Dvd;




            public override void Visit(SyntaxNode node)
            {
                var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
                var linenum = lineSpan.StartLinePosition.Line;
                MemCpu.VNode vN = new MemCpu.VNode(node);
                int op1 = 0, op2 = 0;
                //parser.lineno = linenum + 1;
                Tabs++;
                indents = new String('\t', Tabs);


                base.Visit(node);
                Tabs--;
                Console.WriteLine(node.Kind() + " , " + node);
                switch (node.Kind().ToString())
                {
                    case "Block":
                        //Console.WriteLine("pop " + nodeStk.Pop().ToString());
                        break;
                    case "IdentifierName":
                        Console.WriteLine("-- Visit Identifier " + node.ToString());
                        if ((vN = vNs.Find(vnm => vnm.node == node)) == null)
                        {
                            vN = new MemCpu.VNode(node);
                            vN.vix = vNs.Count;
                            if (node.Kind().ToString() == "NumericLiteralExpression")
                                vN.val = (int)node.GetFirstToken().Value;
                            vNs.Add(vN);
                            //xN.vix = memCpu.vRam.vals.Count;
                            //memCpu.vRam.vals.Add(xN.val);
                            Console.WriteLine(" New    " + node + " vix = " + vN.vix.ToString() + "  Push  " + node.ToString());
                        }
                        else
                            Console.WriteLine(" Found " + node + " vix = " + vN.vix.ToString() + " Push " + node.ToString());
                        VNStk.Push(vN);
                        break;
                    case "NumericLiteralExpression":
                        Console.WriteLine("-- Visit Numeric " + node.ToString());
                        if ((vN = vNs.Find(vnm => vnm.node == node)) == null)
                        {
                            vN = new MemCpu.VNode(node);
                            vN.vix = vNs.Count;
                            vNs.Add(vN);
                            //xN.vix = memCpu.vRam.vals.Count;
                            //memCpu.vRam.vals.Add(xN.val);
                            VNStk.Push(vN);
                            Console.WriteLine(" New    " + node + " vix = " + vN.vix.ToString() + "  Push  " + node.ToString());
                        }
                        else
                        {
                            VNStk.Push(vN);
                            Console.WriteLine(" Found " + node + " vix = " + vN.vix.ToString() + " Push " + node.ToString());
                        }

                        break;
                    case "PredefinedType":
                        //Console.WriteLine("--  " + node.Kind().ToString());
                        break;
                    case "ExpressionStatement":
                        //Console.WriteLine("pop Xpr " + nodeStk.Pop().ToString());
                        break;
                    case "SimpleAssignmentExpression":
                        vN.val = VNStk.Pop().val;
                        VNStk.Peek().val = vN.val;
                        Console.WriteLine("       assign " + vN.node + " = " + vN.val.ToString());
                        break;
                    case "ParenthesizedExpression":
                        nodeStk.Push(node);
                        Console.WriteLine("-- Visit " + nodeStk.Peek().Kind().ToString());
                        break;
                    case "AddExpression":
                        vN.node = node;
                        vN.val = addDel(VNStk.Pop().val, VNStk.Pop().val);
                        Console.WriteLine("-- Sum = " + vN.val.ToString());
                        VNStk.Push(vN);
                        VNStk.Peek().node = node;
                        break;
                    case "SubtractExpression":
                        vN.node = node;
                        vN.val = subDel(VNStk.Pop().val, VNStk.Pop().val);
                        Console.WriteLine("-- Difference = " + vN.val.ToString());
                        VNStk.Push(vN);
                        break;
                    case "MultiplyExpression":
                        vN.node = node;
                        vN.val = mulDel(VNStk.Pop().val, VNStk.Pop().val);
                        Console.WriteLine("-- Product = " + vN.val.ToString());
                        VNStk.Push(vN);
                        break;
                    case "DivideExpression":
                        vN.node = node;
                        vN.val = dvdDel(VNStk.Pop().val, VNStk.Pop().val);
                        Console.WriteLine("-- Quotient = " + vN.val.ToString());
                        VNStk.Push(vN);
                        break;
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

        // begin creates AST and calls SyntaxWalker cEWalker
        //public void begin(String fPath, ListBox lB1in)  //  , CEngine pE
        //{
        //    lB1 = lB1in;
        //    lineno = 0;
        //    //StreamReader sR = new StreamReader(fPath);
        //    //sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
        //    sB = new StringBuilder("main() { y = 2 + 3 * 4 - 4; }");
        //    CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
        //    node = tree.GetRoot();
        //    CEngineWalker cEwalker = new CEngineWalker();
        //    cEwalker.parser = this;
        //    cEwalker.uCwds = uCwds;
        //    cEwalker.Visit(node);
        //}




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
                public new string cond { get { return parent.cond; } }
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
                //public string ckwd { get { return "while"; } }
                //public new string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
                //public new string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
                public int lineno { get { return parent.srcln; } set { parent.srcln = value; } }
                public int loopix;
                public int condix, bodyix, endix;
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
                public new string cond;
                public String ckwd { get { return parent.kwd; } }
                public int lineno { get { return parent.srcln; } }

            }
        }

        [Flags]
        public enum ucdEnum
        {
            //      
            call = 0x8000,  // 
            rtn = 0x4000,  // 
            tna = 0x2000,  // 0x000E combines with cond opers and uses spare opcodes
            fna = 0x1000,  // 0x000F
            eop = 0x0800,  //
            gtr = 0x0410,
            eql = 0x0210,  // !eql = 0x0510
            less = 0x0110,
            pway = 0x0080,  // 
            push = 0x0050,  // may use 0x8040 to stack call parms, or with ALU codes 
            pop = 0x0030,  // 
            etime = 0x0010,  // part of opcode for ALU and compares. decoded also for sequencing
            nop = 0x0000, bnd = 0x0011, bxo = 0x0012, bor = 0x0013, add = 0x0014, sub = 0x0015,
            mpy = 0x0016, dvd = 0x0017, mlo = 0x0018, lsf = 0x0019, rsf = 0x001A, ldm = 0x000E, stm = 0x000F
        }

        //public void ckwds(SyntaxNode node)
        //{
        //    cKwds(node, null, vNs, uCwds);
        //}

        //public void cKwds(SyntaxNode node, LinkedList<CStmnt> sXList, List<VNode> vbls, List<uint> uCwds)
        //{
        //    CStmnt cS = new CStmnt();
        //    cS.name = node.Kind().ToString();
        //    int lp = 0, rp = 0;
        //    char[] semi = new char[] { ';' };
        //    char[] keychars = new char[] { 'i', 'e', 'f', 'w', 'd', 's' };
        //    string[] keywords = new string[] { "=", "(", "{", "}", "if", "else", "for", "while", "do", "switch" };

        //    switch (node.Kind().ToString().Trim())
        //    {
        //        case "if":
        //            CStmnt.CIf cIf = new CStmnt.CIf(cS, node);
        //            //cIf.cond = sBin.ToString().Substring(0, sBin.ToString().IndexOf(')') + 1);
        //            //rplSB(cIf.cond);
        //            //sBin.Remove(0, cIf.cond.Length);
        //            //cIf.cond = rpb.ToString();
        //            //makCond(cIf.cond, vbls, uCwds);
        //            uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.eop | ucdEnum.fna) << 16 | cIf.fcx);
        //            cIf.fcx = uCwds.Count;  // fcx saves cc Ix for false target
        //            uCwds.Add(0); // update after true statement(s)
        //                          //nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);  // ckwds(sXList);  // cIf.tList);
        //            uCwds[cIf.fcx] = (uint)uCwds.Count; // 
        //                                                //if (sBin.ToString().IndexOf("else") >= 0)
        //                                                //{
        //                                                //    sBin.Remove(0, sBin.ToString().IndexOf("else") + 4);
        //                                                //    //cas.Add(ccs.Count);
        //                                                //    //cas.Add(0);
        //                                                //    //ccs.Add((int)(ucdEnum.tna | ucdEnum.fna)); // insert jump over else 
        //                                                //    //cIf.enx = ccs.Count; // ccs 
        //                                                //    //ccs.Add(0);
        //                                                //    //ccs[cIf.fcx] = cas.Count;
        //                                                //    //cKwds(sXList, vbls, Scws);
        //                                                //    //ccs[cIf.enx] = cas.Count; // come here to skip else true cond
        //                                                //}
        //                                                //return;

        //            //  10/5/13
        //            //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        //            //          	      sCwA[2]                                eop ? sCwA[cwCt.qa] : 0
        //            //					  sCwB[0]             sCwB[cwCt.qa + 1]  eop ? 0 : sCwB[cwCt.qa + 1] 
        //            //										  va[sCwA.qa]        pway ? pway@ : etime ? stkCt.qa]
        //            //										  vb[pop ? stkCt.qa : etime ? sCwB.qb : sCwA.qb]                            
        //            //                    cwCt.qa = 2         cwCtA = cwCt.qa + 1 

        //            //makCond(cWhile.cond, vbls, uCwds); // blds opwds for cond eval
        //            //    rplSB(cWhile.cond);
        //            //    cWhile.cond = rpb.ToString();
        //            //    makCond(cWhile.cond, vbls, Scws); // blds opwds for cond eval
        //            //    cWhile.loopix = Scws.Count - 1;
        //            //    Scws[Scws.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
        //            //    cWhile.bodyix = Scws.Count;
        //            //    delimIx = nxtDelim();
        //            //    nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
        //            //    Scws.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
        //            //    Scws[cWhile.loopix] |= (uint)Scws.Count; ;
        //            break;
        //        case "WhileStatement":
        //            CStmnt.CWhile cWhile = new CStmnt.CWhile(cS, node);
        //            //rplSB(cWhile.cond);
        //            //cWhile.cond = rpb.ToString();
        //            //makCond(cWhile.cond, vbls, Scws); // blds opwds for cond eval
        //            return;

        //            cWhile.loopix = uCwds.Count - 1;
        //            uCwds[uCwds.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
        //            cWhile.bodyix = uCwds.Count;
        //            uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
        //            uCwds[cWhile.loopix] |= (uint)uCwds.Count;
        //            //cWhile.condix= mem.Count;
        //            //makCond(cWhile.cond, vbls, Scws, Xcws);// blds opwds for cond eval
        //            //uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
        //            //uCwds[cWhile.loopix] |= (uint)uCwds.Count; ;
        //            return;
        //        //case "while":
        //        //    CWhile cWhile = (CWhile)sXList.Last.Value;
        //        //    rplSB(cWhile.cond);
        //        //    cWhile.cond = rpb.ToString();
        //        //    makCond(cWhile.cond, vbls, Scws); // blds opwds for cond eval
        //        //    cWhile.loopix = Scws.Count - 1;
        //        //    Scws[Scws.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
        //        //    cWhile.bodyix = Scws.Count;
        //        //    delimIx = nxtDelim();
        //        //    nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
        //        //    Scws.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
        //        //    Scws[cWhile.loopix] |= (uint)Scws.Count; ;
        //        //    return cWhile;

        //        case "for": // inits, fcond jumps over blk and post assigns
        //            CStmnt.CFor cFor = new CStmnt.CFor(cS, node);
        //            //inSplitA = cFor.xprn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //            //foreach (string pfx in inSplitA) // 0 or more assignments
        //            //{
        //            //    //rplSB(pfx + ';');
        //            //    //mem_opwds(rpb.ToString(), uCwds, vbls);
        //            //}
        //            // for init done:  leave space in cas to go to cond eval then true cond repeats loop
        //            //rplSB(cFor.cond);
        //            //cFor.cond = rpb.ToString().Trim();
        //            //makCond(cFor.cond, vbls, uCwds); // blds opwds for cond eval
        //            uCwds[uCwds.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
        //            cFor.bodyix = uCwds.Count;
        //            //delimIx = nxtDelim();
        //            //nxtStmnt(ref token, ref inlineSplit, delims, Scws, vbls);
        //            //inSplitA = cFor.post.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //            //foreach (string pfx in inSplitA)
        //            //{
        //            //    //rplSB(pfx + ';');
        //            //    //mem_opwds(rpb.ToString(), uCwds, vbls);
        //            //    //nxtDelim();
        //            //} // body and post-fix done
        //            uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cFor.bodyix); // fcond jumps over body
        //            uCwds[cFor.bodyix - 1] |= (uint)uCwds.Count;
        //            return;
        //        case "do": // tcond at end to jump back
        //                   //CStmnt.doWhile cFor = new CStmnt.
        //                   //doWhile.cond = rpb.ToString();
        //                   //makCond(doWhile.cond, vbls, uCwds);
        //                   //uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.etime | ucdEnum.eop | ucdEnum.tna) << 16 | doWhile.loopix);
        //            return;
        //        case "switch":
        //            CStmnt.CSwitch cSwitch = new CStmnt.CSwitch(cS, node);
        //            //cSwitch.xprn = inSplitA[0];
        //            //cSwitch.srcln = lineno;
        //            //sXList.AddLast(cSwitch);
        //            //MessageBox.Show("Switch expr error");
        //            //getNext();
        //            break;
        //        /* function is a variable that accesses variables to compute the return value for the get accessor.
        //         * Call pushes arguments on the stack that grows downward in vbl memory so the address is formed by 
        //         * adding the index to stack pointer.  The main function pointer is zero so the variables index is
        //         * used to address variables using the same mechanism.  The call bit addresses the function using
        //         * the internal variable value and returns the ALU result value instead of the internal variable value.
        //         */
        //        /* push base | stkct to use as part of call to pass args  call push pway
        //         * push args   push pway
        //         * push rtnct | cwct 
        //         * stkct to base, cwct =fun@   call endop cwct[ocwB]
        //         * dofnctn
        //         * restore counts wrt return to TOS call pop
        //         */
        //        //case "fncall":
        //        //    cS = new CStmnt(lineno, "funcall");
        //        //    sXList.AddLast(cS);
        //        //    if ((lp = sBin.ToString().IndexOf('(')) >= 0)
        //        //        sBin.Remove(0, lp + 1);
        //        //    else
        //        //        MessageBox.Show("call parens error");
        //        //    if ((rp = sBin.ToString().IndexOf(')')) >= 0)
        //        //    {
        //        //        cS.xprn = cvbls[get_vbl(sBin.ToString().Substring(0, lp).Trim(), vbls)].cfnref.sb.ToString();
        //        //        sBin.Remove(0, rp + 1);
        //        //    }
        //        //    else
        //        //        MessageBox.Show("call parens error");
        //        //    if ((rp = sBin.ToString().IndexOf(')')) >= 0)
        //        //        sBin.Remove(0, rp + 1);
        //        //    break;
        //        //case "vasgn":
        //        //    cStmnt = new CStmnt();
        //        //    //rpb.Remove(0, rpb.Length);
        //        //    //rplSB(sBin.ToString().Substring(0, sBin.ToString().IndexOf(';')));
        //        //    mem_opwds(rpb.ToString(), uCwds, vbls);
        //        //    cStmnt.xprn = rpb.ToString();
        //        //    sXList.AddLast(cStmnt);
        //        //    sBin.Remove(0, sBin.ToString().IndexOf(';') + 1);
        //        //    //nxtDelim();
        //        //    return;
        //        default:
        //            break;
        //    } // end while switch 
        //    return;
        //}
        // end cKwds


    }
}
