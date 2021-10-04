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
    public Rop tRop;
    string token = " ";
    public int lineno;
    public int cbcnt = 0;
    string[] inlineSplit;
    //FileStream inStr;
    StreamReader sRdr;
    StringBuilder sBin;
    List<String> OutList = new List<string>();
    public ListBox lB1;
    //public ListBox lB2;
    //public ListBox lB3;
    public List<Cvbl> cVbls = new List<Cvbl>();
    public CStmnt cStmnt;
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
    //public Stack<Rop> opStk = new Stack<Rop>();
    public string seq = "";
    public bool reset;
    public StringBuilder sB;
    //public StringBuilder rpb = new StringBuilder();
    //public List<string> sBT = new List<string>();
    //public Stack<string> sStk = new Stack<string>();
    //List<uint> cList = new List<uint>();
    //public LinkedList<CStmnt> sXList = new LinkedList<CStmnt>();
    //public LinkedListNode<CStmnt> llN;
    //List<String> xS = new List<string>();
    //public List<string> sxl = new List<string>();
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
        //public Stack<SyntaxToken> tknStk = new Stack<SyntaxToken>();
        //public Stack<SyntaxToken> xTstk = new Stack<SyntaxToken>();
        //public Stack<string> txtStk = new Stack<string>();
        public Stack<SyntaxNode> nodeStk = new Stack<SyntaxNode>();
        public Stack<SyntaxNode> xnodeStk = new Stack<SyntaxNode>();
        //public List<SyntaxNode> asgnLst = new List<SyntaxNode>();
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
            Cvbl cV, TOS;
            int op1 = 0, op2 = 0;
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
                    Console.WriteLine("-- pop Identifier " + node.ToString());
                    if ((cV = parser.cVbls.Find(vnm => vnm.name == node.ToString())) == null)
                    {
                        cV = new Cvbl(node.ToString(), parser.cVbls);
                        parser.cVbls.Add(cV);
                    }
                    nodeStk.Pop();
                    break;
                case "NumericLiteralExpression":
                    Console.WriteLine("-- pop Numeric " + node.ToString());
                    if ((cV = parser.cVbls.Find(vnm => vnm.name == node.ToString())) == null)
                    {
                        cV = new Cvbl(node.ToString(), parser.cVbls);
                        cV.val = (Int32.Parse(node.ToString()));
                        parser.cVbls.Add(cV);
                    }
                    nodeStk.Pop();
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
                    var lft = ((BinaryExpressionSyntax)node).Left;
                    var vn = node.ToString();
                    var rgt = ((BinaryExpressionSyntax)node).Right;
                    var oper = ((BinaryExpressionSyntax)node).OperatorToken;
                    switch (lft.Kind().ToString())
                    {
                        case "NumericLiteralExpression":
                        case "IdentifierName":
                            if ((cV = parser.cVbls.Find(vnm => vnm.name == lft.ToString())) != null)
                            {
                                uCwds.Add((uint)(cV.vix << 16));
                                op1 = cV.val;
                            }
                            break;
                        default:
                            if (xnodeStk.Peek() == lft)
                            {
                                op1 = xStk.Pop();
                                xnodeStk.Pop();
                            }
                            else
                                if (xnodeStk.Peek() == rgt)
                            {
                                op2 = xStk.Pop();
                                xnodeStk.Pop();
                                if (xnodeStk.Peek() == lft)
                                {
                                    op1 = xStk.Pop();
                                    xnodeStk.Pop();
                                }
                            }

                            break;
                    }
                    switch (rgt.Kind().ToString())
                    {
                        case "NumericLiteralExpression":
                        case "IdentifierName":
                            if ((cV = parser.cVbls.Find(vnm => vnm.name == rgt.ToString())) != null)
                            {
                                uCwds.Add((uint)cV.vix);
                                op2 = cV.val;
                            }
                            break;
                        default:
                            //if (xnodeStk.Peek() == rgt)
                            { }
                            break;
                    }

                    //uCwds.Add((uint)(get_vbl(xopsA[0], cvblsA) << 16 | get_vbl(xopsA[1], cvblsA))); //  Xcws.Count)
                    //cwDcd.Add("op1 = " + xopsA[0] + " op2 = " + xopsA[1]);
                    switch (oper.ValueText)
                    {
                        case "+":
                            xStk.Push(op1 + op2);
                            xnodeStk.Push(node);
                            uCwds.Add((uint)ucdEnum.add);
                            break;
                        case "-":
                            xStk.Push(op1 - op2);
                            uCwds.Add((uint)ucdEnum.sub);
                            break;
                        case "*":
                            xStk.Push(op1 * op2);
                            uCwds.Add((uint)ucdEnum.mpy);
                            break;
                        case "/":
                            xStk.Push(op1 / op2);
                            uCwds.Add((uint)ucdEnum.dvd);
                            break;
                        case "&":
                            xStk.Push(op1 & op2);
                            uCwds.Add((uint)ucdEnum.bnd);
                            break;
                        case "|":
                            xStk.Push(op1 | op2);
                            uCwds.Add((uint)ucdEnum.bor);
                            break;
                        case "^":
                            xStk.Push(op1 ^ op2);
                            uCwds.Add((uint)ucdEnum.bxo);
                            break;
                        case "==":
                            xStk.Push(op1 == op2 ? 0 : 1);
                            uCwds.Add((uint)ucdEnum.eql);
                            break;
                        case "!=":
                            xStk.Push(op1 != op2 ? 0 : 1);
                            //uCwds.Add((uint)ucdEnum.neq );
                            break;
                        case "<":
                            xStk.Push(op1 != op2 ? 0 : 1);
                            uCwds.Add((uint)ucdEnum.less);
                            break;
                        case "<=":
                            xStk.Push(op1 != op2 ? 0 : 1);
                            //uCwds.Add((uint)ucdEnum.leql);
                            break;
                        case ">":
                            xStk.Push(op1 != op2 ? 0 : 1);
                            uCwds.Add((uint)ucdEnum.less);
                            break;
                        case ">=":
                            xStk.Push(op1 != op2 ? 0 : 1);
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
                case "SimpleAssignmentExpression":
                    Console.WriteLine("pop simpAsgn " + nodeStk.Pop().ToString());
                    var saL = ((AssignmentExpressionSyntax)node).Left;
                    var saR = ((AssignmentExpressionSyntax)node).Right;
                    if ((cV = parser.cVbls.Find(vnm => vnm.name == saL.ToString())) == null)
                        parser.cVbls.Add(cV = new Cvbl(saL.ToString(), parser.cVbls));
                    cV.val = xStk.Pop(); 
                    Console.WriteLine(cV.name + " = " + cV.val.ToString());
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
        //StreamReader sR = new StreamReader(@"C:\Windows.old.001\Users\Karl\Dropbox\ASTEngine\ASTDemo.cs");
        //sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
        sB = new StringBuilder("main() { y = 2 + 3 * 4 - 4; }");
        CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
        node = tree.GetRoot();
        CEngineWalker cEwalker = new CEngineWalker();
        cEwalker.parser = this;
        cEwalker.uCwds = uCwds;
        cEwalker.Visit(node);
    }

    //private void makCond(String cond, List<Cvbl> vbls, List<uint> uCwds)
    //{
    //    if (cond.IndexOf("&&") > 0 || cond.IndexOf("||") > 0)
    //        relBld(cond);
    //    //else
    //    //    mem_opwds(cond, uCwds, vbls);
    //}

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

    public class Cvbl
    {
        public Cvbl(string snm, List<Cvbl> cvblsA)
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
            public string ckwd { get { return "while"; } }
            public new string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
            public new string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
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
            public new string cond { get { return ((WhileStatementSyntax)Node).Condition.ToString(); } }
            public new string xprn { get { return ((WhileStatementSyntax)Node).ToString(); } }
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
        cKwds(node, null, cVbls, uCwds);
    }

    public void cKwds(SyntaxNode node, LinkedList<CStmnt> sXList, List<Cvbl> vbls, List<uint> uCwds)
    {
        CStmnt cS = new CStmnt();
        cS.name = node.Kind().ToString();
        int lp = 0, rp = 0;
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
                if (sBin.ToString().IndexOf("else") >= 0)
                {
                    sBin.Remove(0, sBin.ToString().IndexOf("else") + 4);
                    //cas.Add(ccs.Count);
                    //cas.Add(0);
                    //ccs.Add((int)(ucdEnum.tna | ucdEnum.fna)); // insert jump over else 
                    //cIf.enx = ccs.Count; // ccs 
                    //ccs.Add(0);
                    //ccs[cIf.fcx] = cas.Count;
                    //cKwds(sXList, vbls, uCwds);
                    //ccs[cIf.enx] = cas.Count; // come here to skip else true cond
                }
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
                //makCond(cWhile.cond, cVbls, Xcws);// blds opwds for cond eval
                uCwds.Add((uint)((int)(ucdEnum.eop | ucdEnum.tna | ucdEnum.etime) | cWhile.bodyix)); // fcond jumps over body
                uCwds[cWhile.loopix] |= (uint)uCwds.Count; 
                return;
            //case "while":
            //    CWhile cWhile = (CWhile)sXList.Last.Value;
            //    rplSB(cWhile.cond);
            //    cWhile.cond = rpb.ToString();
                //makCond(cWhile.cond, vbls, uCwds); // blds opwds for cond eval
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
