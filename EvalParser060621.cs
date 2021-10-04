using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;


//  "C:\Users\Karl\Source\Repos\ASTcopy\ASTEngine\ASTEngine\ASTDemo04082018.csC:\Users\Karl\Source\Repos\ASTcopy\ASTEngine\ASTEngine\ASTDemo04082018.cs
//C:\Users\Karl\Source\Repos\ASTcopy\ASTEngine\ASTEngine\ASTDemo04082018.cs

namespace RISCV
{
    public class Parser
    {
        public List<uint> uCwds = new List<uint>();
        List<uint> Xcws = new List<uint>();
        public static char[] xOpers = new char[] { '+', '-', '!', '~', '=', '?', ':', '&', '|', '^', '*', '/', '<', '>', '%' };  // 71, 94 , 364
        char[] cBs = new char[] { '{', '}' };
        char[] wsp = new char[] { };
        public char[] opers = new char[] { '+', '-', '*', '/', '%', '&', '|', '^' };  // '<', '>',, '<', '>' };
        public char[] expOps = new char[] { ';', '=', '+', '-', '!', '~', '(', ')', '?', ':', '&', '|', '^', '*', '/', '<', '>', '%' };
        string[] inSplitA;
        Stack<VN> nStack = new Stack<VN>();
        public Rop tRop;
        public int lineno;
        public int cbcnt = 0;
        //FileStream inStr;
        //StreamReader sRdr;
        //StringBuilder sBin;
        public ListBox lB1;
        public List<XVal> xVals = new List<XVal>();


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
        public string seq = "";
        public bool reset;
        public StringBuilder sB;
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
        // Invoke the delegate and display the output.
        //static string sValIn;
        //static Func<string, int> sVal = s => Int32.Parse(s[0].ToString()) + Int32.Parse(s[1].ToString()) * Int32.Parse(s[2].ToString());
        //string sxpr = "2+3*7";
        static string[] sIn = "2+3*7-8".Split(new char[] { '+', '-', '*', '/' }, StringSplitOptions.RemoveEmptyEntries);
        static List<int> nums = new List<int>();
        //foreach(string s in sIn)
        static Func<string[], int> sVala = s => Int32.Parse(s[0]) + Int32.Parse(s[1]) * Int32.Parse(s[2]) - Int32.Parse(s[3]);
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
            public Stack<SyntaxToken> tknStk = new Stack<SyntaxToken>();
            public Stack<SyntaxToken> xTstk = new Stack<SyntaxToken>();
            public Stack<string> txtStk = new Stack<string>();
            public Stack<SyntaxNode> nodeStk = new Stack<SyntaxNode>();
            public Stack<SyntaxNode> xnodeStk = new Stack<SyntaxNode>();
            public Stack<CStmnt> cSStk = new Stack<CStmnt>();

            VN vN;
            public List<VN> vNs = new List<VN>();
            //XT xT;
            //public List<XT> xTs = new List<XT>();
            List<SyntaxNodeOrToken> ntl;
            //List<SyntaxNode> nL = new List<SyntaxNode>();
            //public Stack<XT> xTStk = new Stack<XT>();
            public Stack<VN> vNStk = new Stack<VN>();
            public List<SyntaxNode> asgnLst = new List<SyntaxNode>();
            public Stack<Int32> xStk = new Stack<Int32>();
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
            public SyntaxNode node;
            public SyntaxToken token;
            public Parser parser;
            public List<uint> uCwds;
            public int TOS;
            //public StringBuilder rpn = new StringBuilder();


            public override void Visit(SyntaxNode node)
            {
                var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);
                var linenum = lineSpan.StartLinePosition.Line;
                XVal xV;
                int op1L = 0, opR = 0;
                parser.lineno = linenum + 1;
                Tabs++;
                indents = new String('\t', Tabs);
                nodeStk.Push(node);
                switch (node.Kind().ToString())
                {
                    case "IdentifierName":
                    case "NumericLiteralExpression":
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
                        Console.WriteLine("push " + node.Kind().ToString());
                        break;
                    case "CompilationUnit":
                    case "ParameterList":
                        break;
                    case "ForStatement":
                        Console.WriteLine("push " + nodeStk.Peek().Kind().ToString());
                        //Console.WriteLine("Visit " + node.ToString());
                        CStmnt cS = new CStmnt();
                        CStmnt.CFor cFor = new CStmnt.CFor(cS, node);
                        break;
                    case "WhileStatement":
                        Console.WriteLine("push " + node.ToString());
                        //Console.WriteLine("Visit " + node.ToString());
                        cS = new CStmnt(linenum, "while");
                        CStmnt.CWhile cWhile = new CStmnt.CWhile(cS, node);
                        var whileCond = ((WhileStatementSyntax)node).Condition;
                        Visit(whileCond);
                        //Visit(node);
                        //parser.cKwds(node, null, parser.cvbls, parser.uCwds);
                        //parser.makCond(node.Condition., parser.cvbls, uCwds); // blds opwds for cond eval
                        //rplSB(cWhile.cond);
                        //cWhile.cond = rpb.ToString();
                        //makCond(cWhile.cond, parser.cvbls, uCwds); // blds opwds for cond eval
                        cWhile.loopix = uCwds.Count;    // - 1;
                                                        //    uCwds[uCwds.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
                                                        //    cWhile.bodyix = uCwds.Count;
                                                        //    delimIx = nxtDelim();
                                                        //    nxtStmnt(ref token, ref inlineSplit, delims, uCwds, vbls);
                                                        //    uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
                                                        //    uCwds[cWhile.loopix] |= (uint)uCwds.Count; ;
                        break;
                    case "DoWhileStatement":
                        Console.WriteLine("push " + node.Kind().ToString());
                        //Console.WriteLine("Visit " + node.ToString());
                        cS = new CStmnt();
                        CStmnt.CDoWhile cDoWhile = new CStmnt.CDoWhile(cS, node);
                        break;
                    case "SwitchStatement":
                        Console.WriteLine("push " + node.Kind().ToString());
                        //Console.WriteLine("Visit " + node.ToString());
                        //parser.cKwds(node, null, parser.cvbls, parser.uCwds);
                        cS = new CStmnt();
                        CStmnt.CSwitch cSwitch = new CStmnt.CSwitch(cS, node);
                        break;
                    case "ParenthesizedExpression":
                        nodeStk.Pop();
                        //Console.WriteLine("push " + nodeStk.Peek().Kind().ToString());
                        break;
                    case "Block":
                        nodeStk.Pop();
                        //Console.WriteLine("push " + nodeStk.Peek().Kind().ToString());
                        break;
                    case "ExpressionStatement":
                        //nodeStk.Push(node);
                        Console.WriteLine("push Xpr " + nodeStk.Peek().ToString());
                        break;
                    case "SimpleAssignmentExpression":
                        //nodeStk.Push(node);
                        Console.WriteLine("push simpAsgn " + nodeStk.Peek().ToString());
                        if (xStk.Count > 0)
                        {
                            Console.WriteLine("xStk not empty " + xStk.Pop().ToString());
                            xStk.Clear();
                        }
                        break;
                    default:
                        break;
                }

                /*

                        else    //  eval
                        {
                            uCwds.Add((uint)(get_vbl(xopsA[0], cvblsA) << 16 | get_vbl(xopsA[1], cvblsA))); //  Xcws.Count)
                            cwDcd.Add("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
                        }
                        do
                        {
                            sx.oper = token;
                            cprec(ref sx);
                            tokenIx = sb2.ToString().IndexOfAny(expOps);
                            if (tokenIx < 0)
                            {
                                if (sb2.ToString().TrimStart().Length == 0)
                                {
                                    uCwds.Add((uint)(sx.opcw | (int)ucdEnum.eop) << 16);
                                    cwDcd.Add(String.Format("{0} " + "{1}", (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0), (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x1f)));
                                    return;
                                }
                            }
                            token = sb2[tokenIx].ToString();
                            xopsA = sb2.ToString().Substring(0, tokenIx >= 0 ? tokenIx : sb2.Length).Split(wsp, StringSplitOptions.RemoveEmptyEntries);
                            switch (xopsA.Length)
                            {
                                case 0: // back to back opers get opnds from stack
                                    uCwds.Add((uint)sx.opcw << 16);
                                    cwDcd.Add(String.Format("{0} " + "{1}", (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0), (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x1f)));
                                    uCwds.Add((int)(ucdEnum.pop | ucdEnum.etime) << 16);
                                    cwDcd.Add(String.Format("{0}", (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0)));
                                    break;
                                case 1: // normal case 1 opnd per oper
                                    if (token == "=")
                                    {
                                        uCwds.Add((uint)((sx.opcw | (int)(ucdEnum.pway | ucdEnum.eop)) << 16 | get_vbl(xopsA[0], cvblsA)));
                                        cwDcd.Add(String.Format("{0 }" + " {1} " + xopsA[0], (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0), (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x1f)));
                                    }
                                    else
                                    {
                                        uCwds.Add((uint)(sx.opcw << 16 | get_vbl(xopsA[0], cvblsA)));
                                        cwDcd.Add(String.Format("{0} " + "{1} " + xopsA[0], (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0), (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x1f)));
                                    }
                                    break;
                                case 2: // push alu, get 2 opnds
                                    // mem[mem.Count - 2] |= ((int)ops.push);
                                    uCwds.Add((uint)(sx.opcw | (int)ucdEnum.push) << 16);
                                    cwDcd.Add(String.Format("{0}" + "{1}", (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0), (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x1f)));
                                    uCwds.Add((uint)(get_vbl(xopsA[0], cvblsA) << 16 | get_vbl(xopsA[1], cvblsA)));
                                    cwDcd.Add("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);  
                                    break;
                                default: // probably an error, expr could not have more than 2 ?????????
                                    break;
                            }
                            sb2.Remove(0, tokenIx + 1);
                        }
                        while (sb2.ToString().Trim().Length > 0);

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
                switch (node.Kind().ToString())
                {
                    case "Block":
                        Console.WriteLine("pop " + nodeStk.Pop().ToString());
                        break;
                    case "IdentifierName":
                        Console.WriteLine("--  Identifier " + node.ToString());
                        if ((xV = parser.xVals.Find(vnm => vnm.name == node.ToString())) == null)
                        {
                            xV = new XVal(node.ToString(), parser.xVals);
                            parser.xVals.Add(xV);
                        }
                        //nodeStk.Pop();
                        break;
                    case "NumericLiteralExpression":
                        Console.WriteLine("--  Numeric " + node.ToString());
                        if ((xV = parser.xVals.Find(vnm => vnm.name == node.ToString())) == null)
                        {
                            xV = new XVal(node.ToString(), parser.xVals);
                            xV.val = (Int32.Parse(node.ToString()));
                            parser.xVals.Add(xV);
                        }
                        //nodeStk.Pop();
                        break;
                    case "PredefinedType":
                        //Console.WriteLine("--  " + node.Kind().ToString());
                        break;
                    case "ExpressionStatement":
                        Console.WriteLine("pop Xpr " + nodeStk.Pop().ToString());
                        break;                    //if (token == "=")   //  putaway rslt
                                                  //{
                                                  //    uCwds.Add((uint)(get_vbl(xopsA[1], cvblsA) << 16 | uCwds.Count)); //get_vbl(xopsA[1], cvblsA)
                                                  //    uCwds.Add((uint)((int)(ucdEnum.pway | ucdEnum.eop) << 16 | get_vbl(xopsA[0], cvblsA)));
                                                  //    cwDcd.Add(String.Format("{0}" + " ALU to " + xopsA[0], (ucdEnum)((uCwds[uCwds.Count - 1] >> 16) & 0x8f0)));
                                                  //    sb2.Remove(0, tokenIx + 1);
                                                  //    return;
                                                  //}
                    case "SimpleAssignmentExpression":
                        Console.WriteLine("pop simpAsgn " + nodeStk.Pop().ToString());
                        //Console.WriteLine("-- Visit " + node.Kind().ToString());
                        var saL = ((AssignmentExpressionSyntax)node).Left;
                        var saR = ((AssignmentExpressionSyntax)node).Right;
                        if ((xV = parser.xVals.Find(vnm => vnm.name == saL.ToString())) == null)
                            parser.xVals.Add(xV = new XVal(saL.ToString(), parser.xVals));
                        //base.VisitBinaryExpression((BinaryExpressionSyntax)saR);
                        //if (xStk.Count == 0)
                        //    cV.val = Int32.Parse(saR.ToString());
                        //else
                        //{
                        xV.val = xStk.Pop();    // (int)tknStk.Pop().Value;
                        if (saR == xnodeStk.Peek())
                            xnodeStk.Pop();
                        //}
                        Console.WriteLine(xV.name + " = " + xV.val.ToString());
                        break;
                    case "ParenthesizedExpression":
                        nodeStk.Push(node);
                        Console.WriteLine("-- Visit " + nodeStk.Peek().Kind().ToString());
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
                        Console.WriteLine("-- Visit   " + node.ToString());
                        this.vN = new VN(node);
                        this.vN.node = node;
                        this.vN.val = vNStk.Pop().val;
                        Console.WriteLine(" Pop " + this.vN.val.ToString());
                        VN vN = new VN(node);
                        vN.val = vNStk.Peek().val;
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        if (ntl[1].IsToken)
                            switch (ntl[1].AsToken().Text)
                            {
                                case "+":
                                    this.vN.val = vNStk.Pop().val + this.vN.val;
                                    Console.WriteLine(" Sum       " + node + " = " + this.vN.val.ToString());
                                    break;
                                case "-":
                                    this.vN.val = vNStk.Pop().val - this.vN.val;
                                    Console.WriteLine(" Difference " + node + " = " + this.vN.val.ToString());
                                    break;
                                case "*":
                                    this.vN.val = vNStk.Pop().val * this.vN.val;
                                    Console.WriteLine(" Product    " + node + " = " + this.vN.val.ToString());
                                    break;
                                default:
                                    break;
                            }
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        var lft = ((BinaryExpressionSyntax)node).Left;
                        var vn = node.ToString();
                        var rgt = ((BinaryExpressionSyntax)node).Right;
                        var oper = ((BinaryExpressionSyntax)node).OperatorToken;
                        switch (rgt.Kind().ToString())
                        {
                            case "NumericLiteralExpression":
                            case "IdentifierName":
                                if ((xV = parser.xVals.Find(vnm => vnm.name == rgt.ToString())) != null)
                                {
                                    uCwds.Add((uint)xV.vix);
                                    opR = xV.val;
                                }
                                break;
                            default:
                                if (xnodeStk.Peek() == rgt)
                                {
                                    opR = xStk.Pop();
                                }
                                break;
                        }
                        switch (lft.Kind().ToString())
                        {
                            case "NumericLiteralExpression":
                            case "IdentifierName":
                                if ((xV = parser.xVals.Find(vnm => vnm.name == lft.ToString())) != null)
                                {
                                    uCwds.Add((uint)(xV.vix << 16));
                                    op1L = xV.val;
                                }
                                break;
                            default:
                                if (xnodeStk.Peek() == lft)
                                {
                                    op1L = xStk.Pop();
                                    xnodeStk.Pop();
                                }
                                else
                                    if (xnodeStk.Peek() == rgt)
                                {
                                    opR = xStk.Pop();
                                    xnodeStk.Pop();
                                    if (xnodeStk.Peek() == lft)
                                    {
                                        op1L = xStk.Pop();
                                        xnodeStk.Pop();
                                    }
                                }

                                break;
                        }
                        //uCwds.Add((uint)(get_vbl(xopsA[0], cvblsA) << 16 | get_vbl(xopsA[1], cvblsA))); //  Xcws.Count)
                        //cwDcd.Add("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
                        //xN = new XN(node);
                        this.vN.node = node;
                        this.vN.val = vNStk.Pop().val;
                        Console.WriteLine(" Pop " + this.vN.val.ToString());
                        //nStruct nstr = new nStruct(xN.node, xNStk.Peek().val);
                        ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                        if (ntl[1].IsToken)
                            switch (ntl[1].AsToken().Text)
                            {
                                case "+":
                                    this.vN.val = vNStk.Pop().val + this.vN.val;
                                    Console.WriteLine(" Sum       " + node + " = " + this.vN.val.ToString());
                                    break;
                                case "-":
                                    this.vN.val = vNStk.Pop().val - this.vN.val;
                                    Console.WriteLine(" Difference " + node + " = " + this.vN.val.ToString());
                                    break;
                                case "*":
                                    this.vN.val = vNStk.Pop().val * this.vN.val;
                                    Console.WriteLine(" Product    " + node + " = " + this.vN.val.ToString());
                                    break;
                                default:
                                    break;
                            }
                        switch (oper.ValueText)
                        {
                            case "+":
                                xStk.Push(op1L + opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.add);
                                break;
                            case "-":
                                xStk.Push(op1L - opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.sub);
                                break;
                            case "*":
                                xStk.Push(op1L * opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.mpy);
                                break;
                            case "/":
                                xStk.Push(op1L / opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.dvd);
                                break;
                            case "&":
                                xStk.Push(op1L & opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.bnd);
                                break;
                            case "|":
                                xStk.Push(op1L | opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.bor);
                                break;
                            case "^":
                                xStk.Push(op1L ^ opR);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.bxo);
                                break;
                            case "==":
                                xStk.Push(op1L == opR ? 0 : 1);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.eql);
                                break;
                            case "!=":
                                xStk.Push(op1L != opR ? 0 : 1);
                                xnodeStk.Push(node);
                                //uCwds.Add((uint)ucdEnum.neq );
                                break;
                            case "<":
                                xStk.Push(op1L != opR ? 0 : 1);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.less);
                                break;
                            case "<=":
                                xStk.Push(op1L != opR ? 0 : 1);
                                xnodeStk.Push(node);
                                //uCwds.Add((uint)ucdEnum.leql);
                                break;
                            case ">":
                                xStk.Push(op1L != opR ? 0 : 1);
                                xnodeStk.Push(node);
                                uCwds.Add((uint)ucdEnum.less);
                                break;
                            case ">=":
                                xStk.Push(op1L != opR ? 0 : 1);
                                xnodeStk.Push(node);
                                //uCwds.Add((uint)ucdEnum.geql);
                                break;
                            default:
                                break;
                        }
                        if (xStk.Count > 0)
                            Console.WriteLine("-- Result   " + xStk.Peek());// stack mt exception
                                                                            //nodeStk.Pop();
                                                                            //break;
                                                                            //case "EqualsExpression":
                                                                            //case "NotEqualsExpression":
                                                                            //case "GreaterThanExpression":
                                                                            //case "LessThanExpression":
                                                                            //case "LessThanOrEqualExpression":
                                                                            //case "GreaterThanOrEqualExpression":
                                                                            //Console.WriteLine("-- Visit   " + nodeStk.Peek().Kind().ToString());
                                                                            //xStk.Push(-1);
                                                                            //Console.WriteLine("Result value = " + xStk.Pop(), node);
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
                        parser.ckwds(node);
                        break;
                    default:
                        Console.WriteLine("  Unhandled Node: " + node.Kind().ToString());
                        break;
                }
            }


            //public override void VisitToken(SyntaxToken token)  // called by VisitNode
            //{
            //    var lineSpan = token.SyntaxTree.GetLineSpan(token.Span);
            //    var linenum = lineSpan.StartLinePosition.Line;
            //    parser.lineno = linenum + 1;
            //    Console.WriteLine(token.Kind().ToString() + "   " + token.ValueText);
            //    var indents = new String('\t', Tabs);
            //    tknStk.Push(token);
            //    Cvbl cV = new Cvbl(token.ValueText, parser.cVbls);

            //    switch (token.Kind().ToString())
            //    {
            //        case "NumericLiteralToken":
            //            txtStk.Push(token.ValueText);
            //            if ((cV = parser.cVbls.Find(vnm => vnm.name == token.ValueText)) == null)
            //            {
            //                cV = new Cvbl(token.ValueText, parser.cVbls);
            //                cV.val = (Int32)token.Value;
            //                parser.cVbls.Add(cV);
            //            }
            //            break;
            //        case "IdentifierToken":
            //            if ((cV = parser.cVbls.Find(vnm => vnm.name == token.ValueText)) == null)
            //            {
            //                cV = new Cvbl(token.ValueText, parser.cVbls);
            //                parser.cVbls.Add(cV);
            //            }
            //            break;
            //        case "OpenParenToken":
            //        case "CloseParenToken":
            //            break;
            //        case "SemicolonToken":
            //            break;
            //        case "WhileKeyword":
            //            break;
            //        default:
            //            //tknStk.Push(token);
            //            break;
            //    }
            //}
        }

        // begin creates AST and calls SyntaxWalker cEWalker
        public void begin(String fPath, ListBox lB1in)  //  , CEngine pE
        {
            lB1 = lB1in;
            lineno = 0;
            StreamReader sR = new StreamReader(@"C:\Windows.old.001\Users\Karl\Dropbox\ASTEngine\ASTDemo.cs");
            sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
            //sB = new StringBuilder("main() { y = 2 + 3 * 4 - 4; }");
            CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
            node = tree.GetRoot();
            CEngineWalker cEwalker = new CEngineWalker();
            cEwalker.parser = this;
            cEwalker.uCwds = uCwds;
            cEwalker.Visit(node);
        }

        private void makCond(String cond, List<XVal> vbls, List<uint> uCwds)
        {
            if (cond.IndexOf("&&") > 0 || cond.IndexOf("||") > 0)
                relBld(cond);
            //else
            //    mem_opwds(cond, uCwds, vbls);
        }

        public string relBld(string xpr)
        {
            StringBuilder orS = new StringBuilder();
            int orix = 0, ssix = 0;
            if ((orix = xpr.Substring(ssix).IndexOf("||")) < 0)
            {
                while (xpr.Length >= ssix)
                {
                    orix = xpr.Substring(ssix).IndexOf("&&");
                    if (orix < 0)
                    {
                        orS.Append("if(" + xpr.Substring(ssix) + ")");
                        break;
                    }
                    else
                        orS.Append(" if(" + xpr.Substring(ssix, orix) + ") ");
                    ssix += orix + 2;
                }
                return orS.ToString();

            }
            else
            {
                while (xpr.Length >= ssix)
                {
                    orix = xpr.Substring(ssix).IndexOf("||");
                    if (orix < 0)
                    {
                        orS.Append("else if(" + xpr.Substring(ssix) + ")");
                        break;
                    }
                    else
                        orS.Append(" else if(" + xpr.Substring(ssix, orix) + ") ");
                    ssix += orix + 2;
                }
                return orS.ToString();
            }
        }

        public class XVal
        {
            public XVal(string snm, List<XVal> cvblsA)
            {
                name = snm;
                vix = cvblsA.Count;
            }
            public int val
            {
                get { return vval; }
                set { vval = value; }
            }
            public string name; public int vval; public string type;
            public int scn_ix; public int vix = -1;
        }

        //public class VN
        //{
        //    public VN(SyntaxNode sN)
        //    {
        //        //this.token = tkn.ToString();
        //        node = sN;
        //        if (sN.Kind().ToString() == "NumericLiteralExpression")
        //        {
        //            vval = Int32.Parse(sN.ToString());
        //            asgnd = true;
        //        }
        //    }
        //    public string name
        //    {
        //        get { return node.ToString(); }
        //    }

        //    public int val
        //    {
        //        get { return vval; }
        //        set
        //        {
        //            if (node.Kind().ToString() != "NumericLiteralExpression")
        //            {
        //                vval = value;
        //                asgnd = true;
        //            }
        //        }
        //    }
        //    public string token;
        //    public SyntaxNode node;
        //    public int vval, scn_ix, vix = -1;
        //    public bool asgnd = false, cmpr;

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
                //public new string cond { get { return parent.cond; } }
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
                public string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
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
        }

        [Flags]
        public enum ucdEnum
        {
            //      
            call = 0x8000,  // 
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
            nop = 0x00000000, bnd = 0x00110000, bxo = 0x00120000, bor = 0x00130000
                , add = 0x00140000, sub = 0x00150000, mpy = 0x00160000, dvd = 0x00170000
                , mlo = 0x00180000, lsf = 0x00190000, rsf = 0x001A0000, ldm = 0x000E0000
                , stm = 0x000F0000
        }

        public void ckwds(SyntaxNode node)
        {
            cKwds(node, null, xVals, uCwds);
        }

        public void cKwds(SyntaxNode node, LinkedList<CStmnt> sXList, List<XVal> vbls, List<uint> uCwds)
        {
            CStmnt cS = new CStmnt();
            cS.name = node.Kind().ToString();
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
                    uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.eop | ucdEnum.fna) | cIf.fcx);
                    cIf.fcx = uCwds.Count;  // fcx saves cc Ix for false target
                    uCwds.Add(0); // update after true statement(s)
                                  //nxtStmnt(ref token, ref inlineSplit, delims, uCwds, vbls);  // ckwds(sXList);  // cIf.tList);
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
                                                        //    //cKwds(sXList, vbls, uCwds);
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

                //makCond(cWhile.cond, vbls, uCwds); // blds opwds for cond eval
                //    rplSB(cWhile.cond);
                //    cWhile.cond = rpb.ToString();
                //    makCond(cWhile.cond, vbls, uCwds); // blds opwds for cond eval
                //    cWhile.loopix = uCwds.Count - 1;
                //    uCwds[uCwds.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
                //    cWhile.bodyix = uCwds.Count;
                //    delimIx = nxtDelim();
                //    nxtStmnt(ref token, ref inlineSplit, delims, uCwds, vbls);
                //    uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
                //    uCwds[cWhile.loopix] |= (uint)uCwds.Count; ;
                case "WhileStatement":
                    CStmnt.CWhile cWhile = new CStmnt.CWhile(cS, node);
                    cWhile.loopix = uCwds.Count;    //   - 1;
                    uCwds.Add((int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime)); // fcond jumps over body
                    cWhile.bodyix = uCwds.Count;
                    uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cWhile.bodyix)); // fcond jumps over body
                    uCwds[cWhile.loopix] |= (uint)uCwds.Count;
                    cWhile.condix = uCwds.Count;
                    makCond(cWhile.cond, xVals, Xcws);// blds opwds for cond eval
                    uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cWhile.bodyix)); // fcond jumps over body
                    uCwds[cWhile.loopix] |= (uint)uCwds.Count;
                    return;
                //case "while":
                //    CWhile cWhile = (CWhile)sXList.Last.Value;
                //    rplSB(cWhile.cond);
                //    cWhile.cond = rpb.ToString();
                //    makCond(cWhile.cond, vbls, uCwds); // blds opwds for cond eval
                //    cWhile.loopix = uCwds.Count - 1;
                //    uCwds[uCwds.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime) << 16; // fcond jumps over body
                //    cWhile.bodyix = uCwds.Count;
                //    delimIx = nxtDelim();
                //    nxtStmnt(ref token, ref inlineSplit, delims, uCwds, vbls);
                //    uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) << 16 | cWhile.bodyix)); // fcond jumps over body
                //    uCwds[cWhile.loopix] |= (uint)uCwds.Count; ;
                //    return cWhile;

                case "for": // inits, fcond jumps over blk and post assigns
                    CStmnt.CFor cFor = new CStmnt.CFor(cS, node);
                    //inSplitA = cFor.xprn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string pfx in inSplitA) // 0 or more assignments
                    {
                        //rplSB(pfx + ';');
                        //mem_opwds(rpb.ToString(), uCwds, vbls);
                    }
                    // for init done:  leave space in cas to go to cond eval then true cond repeats loop
                    //rplSB(cFor.cond);
                    //cFor.cond = rpb.ToString().Trim();
                    //makCond(cFor.cond, vbls, uCwds); // blds opwds for cond eval
                    uCwds[uCwds.Count - 1] |= (int)(ucdEnum.eop | ucdEnum.fna | ucdEnum.etime); // fcond jumps over body
                    cFor.bodyix = uCwds.Count;
                    //delimIx = nxtDelim();
                    //nxtStmnt(ref token, ref inlineSplit, delims, uCwds, vbls);
                    inSplitA = cFor.post.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string pfx in inSplitA)
                    {
                        //rplSB(pfx + ';');
                        //mem_opwds(rpb.ToString(), uCwds, vbls);
                        //nxtDelim();
                    } // body and post-fix done
                    uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cFor.bodyix); // fcond jumps over body
                    uCwds[cFor.bodyix - 1] |= (uint)uCwds.Count;
                    return;
                case "do": // tcond at end to jump back
                           //CStmnt.doWhile cFor = new CStmnt.
                           //doWhile.cond = rpb.ToString();
                           //makCond(doWhile.cond, vbls, uCwds);
                           //uCwds[uCwds.Count - 1] |= (uint)((int)(ucdEnum.etime | ucdEnum.eop | ucdEnum.tna) << 16 | doWhile.loopix);
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


    }
    
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

        //public void Build(ref RISCV rISCV, string fPath, ListBox lB1in, Parser pIn)
        //{
        //    rISCV.Mem.iList.Add(0);
        //    Romar = 0;
        //    Idcd[Romar] = 0;
        //    //opL = new XVal(node, 0);
        //    ListBox lB1 = lB1in;
        //    //int lineno = 0;
        //    //StreamReader sR = new StreamReader(fPath);
        //    StreamReader sR = new StreamReader(@"C:\Users\Karl\Source\Demo100422020.cs");
        //    StringBuilder sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
        //    //sB = new StringBuilder("main() { y = 2 + 3 + 4 - 5; x = y - 6; }");
        //    //sB = new StringBuilder("y = 2 + 3 + 4 - 5; x = y - 6;");
        //    CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
        //    SyntaxNode node = tree.GetRoot();
        //    CEngineWalker cEwalker = new CEngineWalker();
        //    cEwalker.parser = pIn;
        //    cEwalker.Visit(node);
        //    Console.WriteLine(tree);
        //    return;
        //    //decode();
        //}

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
                public Parser.VN vN;
                public List<Parser.VN> vNs = new List<Parser.VN>();
                public Stack<Parser.VN> vNStack = new Stack<Parser.VN>();
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
                            vN.vval = vNStack.Pop().val;
                            vN.node = vNStack.Pop().node;
                            vN.vix = vNs.Find(vnm => vnm.node == vN.node).vix;
                            vNs[vN.vix].val = vN.val;
                            //Console.WriteLine(" Pop   assign " + xNStack.Peek().node + " result = " + xN.val.ToString());
                            Console.WriteLine("       assign " + vN.node + ".value = " + vN.val.ToString());
                            break;
                        case "IdentifierName":
                        case "NumericLiteralExpression":
                            Console.WriteLine(" Pop   " + node.Kind() + "    " + node);
                        if (node.GetFirstToken().Text  == "main"  )
                            break;
                            if ((vN = vNs.Find(vnm => vnm.node == node)) == null)
                            {
                                vN = new Parser.VN(node)
                                {
                                    vix = vNs.Count
                                };
                                if (node.Kind().ToString() == "NumericLiteralExpression")
                                    vN.val = (int)node.GetFirstToken().Value;
                                vNs.Add(vN);
                                //xN.vix = memCpu.vRam.vals.Count;
                                //memCpu.vRam.vals.Add(xN.val);
                                Console.WriteLine(" New    " + node + " vix = " + vN.vix.ToString() + "  Push  " + node.ToString());
                            }
                            else
                            {
                                Console.WriteLine(" Found " + node + " vix = " + vN.vix.ToString() + " Push " + node.ToString());
                            }
                            vNStack.Push(vN);
                            break;
                        case "AddExpression":
                        case "SubtractExpression":
                        case "MultiplyExpression":
                            vN = new Parser.VN(node)
                            {
                                //Stack<Parser.XN> xNStk = new Stack<Parser.XN>();

                                node = node,
                                val = vNStack.Pop().val
                            };
                            Console.WriteLine(" Pop " + vN.val.ToString());
                            //RISCV.NStruct nstr = new RISCV.NStruct(xN.node, xNStack.Peek().val);
                            ntl = new List<SyntaxNodeOrToken>(node.ChildNodesAndTokens());
                            if (ntl[1].IsToken)
                                switch (ntl[1].AsToken().Text)
                                {
                                    case "+":
                                        vN.val = vNStack.Pop().val + vN.val;
                                        Console.WriteLine(" Sum       " + node + " = " + vN.val.ToString());
                                        break;
                                    case "-":
                                        vN.val = vNStack.Pop().val - vN.val;
                                        Console.WriteLine(" Difference " + node + " = " + vN.val.ToString());
                                        break;
                                    case "*":
                                        vN.val = vNStack.Pop().val * vN.val;
                                        Console.WriteLine(" Product    " + node + " = " + vN.val.ToString());
                                        break;
                                    default:
                                        break;
                                }
                            vNStack.Push(new Parser.VN(node));
                            vNStack.Peek().val = vN.val;
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
                            Console.WriteLine("             Pop " + vN.val.ToString());
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
}
