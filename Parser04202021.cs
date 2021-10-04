using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using RISCVE;



namespace AST101
{
    //  "C:\Users\Karl\Source\Repos\ASTcopy\ASTEngine\ASTEngine\ASTDemo04082018.csC:\Users\Karl\Source\Repos\ASTcopy\ASTEngine\ASTEngine\ASTDemo04082018.cs
    //C:\Users\Karl\Source\Repos\ASTcopy\ASTEngine\ASTEngine\ASTDemo04082018.cs

    public class MemCpu
    {
        public MemCpu()
        {
            Parser vP = new Parser();
        }

        public RVE rISCVE = new RVE();

        public List<string> lB1 = new List<string>();
        //List<string> cwOps = new List<string>();
        public Parser parser;
        public StkRam cStk = new StkRam(new uint[64]);
        public ValRam vRam = new ValRam();
        public CtlRom sCwA = new CtlRom();
        //Parser.CNode cV;
        //public List<Parser.CNode> cVals;
        AdrCtl aC = new AdrCtl();
        public Alu alu = new Alu();
        int ix1, ix2;
        char[] bkts = new char[] { '[', ']' };
        uint[] nMsk = new uint[33]  {0x0000, 0x0001, 0x0003, 0x0007, 0x000f,
                                               0x001f, 0x003f, 0x007f, 0x00ff,
                                               0x01ff, 0x03ff, 0x07ff, 0x0fff,
                                               0x1fff, 0x3fff, 0x7fff, 0xffff,
                                               0x0001ffff, 0x0003ffff, 0x0007ffff, 0x000fffff,
                                               0x001fffff, 0x003fffff, 0x007fffff, 0x00ffffff,
                                               0x01ffffff, 0x03ffffff, 0x07ffffff, 0x0fffffff,
                                               0x1fffffff, 0x3fffffff, 0x7fffffff, 0xffffffff};
        public void ipl(System.Windows.Forms.ListBox lBin, List<uint> mem)
        {
            rISCVE.rd = 0;
            //parser.cEWalker.cVals = parserIn.cEWalker.cVals;
            //mem.Add(0);
            sCwA = new CtlRom();
            sCwA.uCWds.Add(0);
            sCwA.uCWds.Add(0);
            foreach (uint i in mem)
            {
                sCwA.uCWds.Add(i);
            }

            lBin.Items.Add("Starting");


            Run(parser);
            foreach (string s in lB1)
                lBin.Items.Add(s);
        }

        public class AdrCtl
        {
        }

        uint mk_fld(string fld, out string[] ix)
        {
            ix = fld.Trim().Split(bkts, StringSplitOptions.RemoveEmptyEntries);
            xParse(ix[1], out ix1);
            xParse(ix[2], out ix2);
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

        private bool xParse(string src, out int xval)
        {
            if (src.Length > 2 && (src[1] == 'x' || src[1] == 'X'))
            {
                if (!int.TryParse(src.Substring(2), NumberStyles.HexNumber, null, out xval))
                {
                    MessageBox.Show("num Parse error");
                    return false;
                }
            }
            else
            {
                if (!int.TryParse(src, out xval))
                {
                    MessageBox.Show("num Parse error");
                    return false;
                }
            }
            return true;
        }

        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        //                    sCwA[2]                                eop ? sCwA[cwCt.qb] : 0
        //                    sCwB[0]              sCwB[cStk.qb + 1] eop ? 0 : sCwB[cStk.qb + 1] 
        //                                         va[sCwA.qa]       pway ? pway@ 
        //                                                            : etime ? cStk.qa
        //                                         vb[pop ? cStk.qa
        //                                          : etime ? sCwA.qb 
        //                                           : sCwA.qb]                            
        //                    cwCt.qa = 2		   cwCtA = cwCt.qa + 1 

        void nxtV(bool etime, bool eop, bool pway, bool tna, bool fna, bool gtr, bool eql, bool less, bool push, bool pop, bool call, bool rtn)
        {
            //vadra = (pop ? (cStk.qa + 1) : 0)
            //        | ((etime && !pway) || (pway && call) ? cStk.qa : 0)
            //        | ((pway && !call) ? sCwA.qb & 0xffff : 0)
            //        | ((!etime && !pway && !call) ? sCwA.qa >> 16 : 0);
            //vadrb = (UInt16)((pop ? cStk.qa : 0)
            //        | ((etime || call) && !endop ? sCwA.qb : 0)
            //        | ((etime || call) && endop ? sCwA.qa : 0));
            //sadra = (UInt16)((!etime && !eop ? (cStk.qb == 0 ? 2 : cStk.qb) : 0)
            //        | ((etime && (cmet || call) ? sCwA.qb : 0)
            //        | ((endop || push) ? (cStk.qb + 1) : 0)
            //        | (endop ? cStk.qb + 1 : 0)));
            //sadra = (UInt16)((!etime && !eop ? cStk.qb == 0 ? 2 : cStk.qb : 0)
            //        | (push ? cStk.qb + 1 : 0)
            //        | ((etime && (endop || push)) ? cmet || call ? (UInt16)sCwA.qb : (UInt16)(cStk.qb + 1) : 0)
            //        | (!etime && endop ? cStk.qb + 1 : 0));
            //sadrb = (UInt16)((etime && (endop || push)) ? 0 : cStk.qb + 1);

        }

        public void Run(Parser parserIn)
        {
            cStk.adra = 0;
            cStk.wrena = true;
            cStk.adrb = 1;
            cStk.wrenb = true;
            cStk.dina = ((uint)vRam.vals.Capacity - 1);
            cStk.dinb = 0;
            cStk.clka();
            cStk.clkb();

            Parser parser = parserIn;
            Parser.CEngineWalker cEWalker = parser.cEWalker;
            Alu alu = new Alu();
            int cycle = 0;
            uint callPtr = 0;
            string fmt;
            while (lB1.Count < 200)
            {
                if (rtn && !call)
                    return;
                //           nxtV(etime, endop, pway, tjmp, fjmp, gtr, eql, less, push, pop, call, rtn);
                // callCt, base;  stkCt, cwCt;  callCt is the call stack ptr
                vadra = pway ? (sCwA.qb & 0xffff) + (cStk.qb >> 16)
                   : !etime && !call ? (sCwA.qa >> 16) + (cStk.qb >> 16)
                   : etime && call ? cStk.qa
                   : pop ? cStk.qa + 1
                   : etime ? cStk.qa : 0;
                vadrb = pop ? cStk.qa
                    : (etime || call) && !endop ? sCwA.qb + (cStk.qb >> 16)
                    : (!etime && !call) || endop ? (sCwA.qa & 0xffff) + (cStk.qb >> 16) : 0;
                sadra = (ushort)(!etime && !endop ? (cStk.qb & 0xffff) == 0 ? 2 : cStk.qb + 1
                    : etime ? (cmet || call && endop && !rtn) ? sCwA.qb
                    : (endop || push) ? call ? 0 : (cStk.qb + 1) : 0 : cStk.qb + 1);
                sadrb = (ushort)(etime && endop || push && !call || !etime && pway ? 0 : cStk.qb + 1);


                string op = ((Parser.UcdEnum)((sCwA.qb) & 0x01f0000)).ToString("F");
                fmt = String.Format("cycle = {0} {1}{2}{3}{4}{5}{6} Op1 = {7}; {8} Op2 = {9}; Alu = {10} {11} {12}"
                 , cycle, (Parser.UcdEnum)(sCwA.qb & 0xfff0), (tjmp ? ",tna " : ""), (fjmp ? ",fna " : "") // 0, 1, 2, 3
                 , (gtr ? ",gtr " : ""), (eql ? ",eql " : ""), (less ? ",less" : "") // 4, 5, 6
                 , vRam.qa, ((Parser.UcdEnum)((sCwA.qb) & 0x001f0000)).ToString(), vRam.qb, alu.qa// 7, 8, 9
                 , wrtVa ? (("; wrt " + alu.qa + " to ") + ((pway && !call) ? (int)vadra < vRam.vals.Count ? "need name" : "vbls[" + vadra + "]" : "TOS")) : ""
                 //                                                                  ----------------------------------------------------
                 //                                          --------------------------------------------------------------------------- 
                 // ------------------------------------------------------------------------------------------------------------------------
                 , cmet ? " cmet " : ""); //
                lB1.Add(fmt);
                Console.WriteLine(fmt);
                if (lB1.Count >= 50)
                    return;
                //c0  
                sCwA.adra = (ushort)sadra;   // (((UInt16)cStk.qb) == 0 ? 2 : cStk.qb + 1);
                sCwA.adrb = (ushort)sadrb;  // (endop ? 0 : cStk.qb + 1);
                                            //AdrCtl myac = new AdrCtl();
                                            //vbls.ac = myac;
                                            //myac.vbls = vbls;
                                            //int x = vbls.ac.vbls.vbls.Count;
                                            // callPtr, base;  stkPtr, cwPtr;
                                            // callPtr, stkPtr; base, cwPtr;
                                            // caller pushes args, uses caller base to get args, pushes parms to stk
                                            // call || push for first arg push call stk stk ct and base;  then push at endop new base and cwCt
                                            // at endop stkCt is new base, fn.cwix is new cwCt
                nxtV(etime, endop, pway, tjmp, fjmp, gtr, eql, less, push, pop, call, rtn);

                //   cStk.qa [callPtr][base]   cStk.qb [stkCt][cwCt]
                callPtr = call && !rtn && endop ? (cStk.qa >> 16)
                    : call && rtn ? (cStk.qa >> 16)
                    : cStk.qa >> 16;
                cStk.adra = call && push ? callPtr + 2 : callPtr;
                cStk.adrb = call ? (push || endop) ? callPtr + 1 : callPtr - 1 : callPtr + 1; // always calllPtr + 1 ????????
                cStk.wrena = call && rtn ? false : true;
                cStk.wrenb = call && rtn ? false : true;
                cStk.dina = pop ? cStk.qa + 1
                    : call ? push ? (uint)(((callPtr & 0xffff) + 2) << 16) | (ushort)(cStk.qa - 1)
                    : rtn ? (uint)(((callPtr & 0xffff) - 2) << 16) | (ushort)(cStk.qa - 1)
                    : (uint)((callPtr & 0xffff) << 16) | (ushort)(cStk.qa - 1)
                    : push ? cStk.qa - 1 : cStk.qa;
                cStk.dinb = call && endop && !rtn ? cStk.qa << 16 | (ushort)sCwA.qb : cStk.qb & 0xffff0000
                    | (ushort)((cmet || call && endop && !rtn) ? sCwA.qb
                    : cStk.qb + 1);
                //: endop ? 0 : cStk.qb + 1;
                //(endop || push) ? call ? 0 : (cStk.qb + 1) : 0);
                //((push || (call && !rtn) ? (cStk.qa - 0x00010000) : pop ? (cStk.qa + 0x00010000) : cStk.qa
                //| (call && endop && !rtn ? (cStk.qa & 0xffff0000) | (UInt16)sCwA.qb
                //: (cStk.qb >> 16) << 16) | (UInt16)(cStk.qb == 0 ? 2 : cmet ? sCwA.qb : cStk.qb + 1)));
                //callPtr = call ? push ? (cStk.qa >> 16) + 2 : rtn ? (cStk.qa >> 16) - 2 : (cStk.qa >> 16) : (cStk.qa >> 16);
                //cStk.adra = callPtr;
                //cStk.adrb = call && !endop ? callPtr - 1 : callPtr + 1;
                ////| (call && endop && rtn ? callPtr + 1 : 0);
                //cStk.wrena = call && rtn ? false : true;
                //cStk.wrenb = call && rtn ? false : true;
                //cStk.dina = callPtr << 16 | (UInt16)(push || (call && !rtn) ? cStk.qa - 1 : pop ? cStk.qa + 1 : cStk.qa);
                //cStk.dinb = call && endop && !rtn ? ((cStk.qa) << 16) | (UInt16)(sCwA.qb) : ((cStk.qb >> 16) << 16)
                //    | (UInt16)((cStk.qb == 0 ? 2 : cmet ? sCwA.qb : cStk.qb + 1));
                //vbls.dina = Alu.qa;
                vRam.adra = (ushort)vadra;
                vRam.adrb = (ushort)vadrb;
                vRam.wrena = wrtVa ? true : false;
                if (wrtVa)
                { }
                sCwA.clka();
                sCwA.clkb();
                vRam.clka();
                vRam.clkb();
                //if (sCwA.uCWds .qa == 0xffffffff || sCwA.qb == 0xffffffff)
                //    return;
                cStk.clka();
                cStk.clkb();
                cycle++;
                uint dcd = (sCwA.qb >> 16);
                //   sCwA.adra = 0; sCwA.adrb = 0; vbls.adra = 0; vbls.adrb = 0;
                //_ = ((Parser.UcdEnum)(dcd & 0XFFF0)).ToString();
                //   sCwA.adra = 0; sCwA.adrb = 0; vbls.adra = 0; vbls.adrb = 0;
                //string aluCode = sCwA.uCWds[(int)sadra].ToString();  //, sCwd, oCwd;

                //____________________|c0|________________|c0|_______________|c0|_______________|c0|___
                //                    sCwA[2]                                eop ? sCwA[cwCt.qa] : 0
                //                    sCwB[0]             sCwB[cStk.qb + 1]  eop ? 0 
                //                                                            : sCwB[cStk.qb + 1] 
                //                                        va[sCwA.qa]        pway ? pway@ 
                //                                                            : etime ? cStk.qa
                //                                        vb[pop ? cStk.qa
                //                                         : etime ? sCwA.qb 
                //                                         : sCwA.qb]                            
                //                    cwCt.qa = 2		  cwCtA = cwCt.qa + 1 

                //string a = "adra", b = "adrb", typ, nm;
                //int flgs = (int)(Parser.ops.etime |Parser.ops.pway | Parser.ops.eop);

                string aluCode = ((Parser.UcdEnum)((int)(sCwA.qb >> 16) & 0X1F)).ToString();

                //    string curcbits = sCwA.cwAlways[sCwA.cwAlways.Count - 1];
                //sadra = sadrA; sadrb = sadrB; vadrb = (uint)vadrB;
                //if (vbls.adra != vadrA || sCwA.adra != sadrA)
                //{ }
            }
        }
        //call = 0x8000,  // 
        //rtn = 0x4000,  // 
        //tna = 0x2000,  // 0x000E combines with cond opers and uses spare opcodes
        //fna = 0x1000,  // 0x000F
        //eop = 0x0800,  //
        //gtr = 0x0410,
        //eql = 0x0210,  // !eql = 0x0510
        //less = 0x0110,
        //pway = 0x0080,  // 
        //push = 0x0050,  // may use 0x8040 to stack call parms, or with ALU codes 
        //pop = 0x0030,  // 
        //etime = 0x0010,

        //  10/5/13
        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        //          	      sCwA[2]                                eop ? sCwA[cwCt.qa + 1] : 0
        //					  sCwB[0]             sCwB[cwCt.qa + 1]  eop ? 0 : sCwB[cwCt.qa + 1] 
        //										  va[sCwA.qa]        pway ? pway@ : etime ? stkCt.qa]
        //										  vb[pop ? stkCt.qa : etime ? sCwB.qb : sCwA.qb]                            
        //                    cwCt.qa = 2         cwCtA = cwCt.qa + 1 
        ///* push base | stkct to use as part of call to pass args  call push pway
        // * push args   push pway
        // * push rtnct | cwct 
        // * stkct to base, cwct =fun@   call endop cwct[ocwB]
        // * dofnctn
        // * restore counts wrt return to TOS call pop   

        //public bool lesscnd
        //{ get { return less & Alu.alb > 0 ? true : false; } }
        //public bool gtrcnd
        //{ get { return gtr & Alu.agb > 0 ? true : false; } }
        //public bool eqlcnd
        //{ get { return eql & Alu.aeb > 0 ? true : false; } }

        /*
        public int sadrA
        { get { return endop || push ? cmet ? (int)(sCwA.qb & 0x0000FFFF) : cStk.qb + 1 : etime ? 0 : cStk.qb == 0 ? 2 : cStk.qb; } }
        public int sadrB
        { get { return etime ? endop ? 0 : cStk.qb + 1 : cStk.qb == 0 ? 2 : cStk.qb + 1; } }
        public int nxtCw
        { get { return cStk.qb == 0 ? 2 : cmet ? (int)(sCwA.qb & 0x0000FFFF) : cStk.qb + 1; } }
        public int vadaa
        { get { return (pop ? (int)cStk.qa + 1 : pwaybit ? sCwA.qb & 0xffff : etime ? (int)cStk.qa : sCwA.qa >> 16); } }
        public int vada
        { get { return pop ? call ? cStk.qa + 2 : (int)cStk.qa + 1 : pwaybit ? sCwA.qb & 0xffff : etime ? (int)cStk.qa : (sCwA.qa >> 16) + ((cStk.qa & 0xffff000) >> 16); } }
        public int vadbb
        { get { return pop ? (int)cStk.qa : etime ? endop ? 0 : (int)sCwA.qb & 0xFFFF : (int)sCwA.qa & 0xFFFF; } }
        public int vadb
        { get { return pop ? call ? cStk.qa + 1 : (int)cStk.qa : etime ? endop ? 0 : (int)sCwA.qb & 0xFFFF : (int)(sCwA.qa & 0xFFFF) + ((cStk.qa & 0xffff000) >> 16); } }

        public int wrtVa
        { get { return etime ? (pop || tjmp || fjmp) ? 0 : 1 : pwaybit ? 1 : 0; } }
        */
        // */

        private uint sadra, sadrb, vadra, vadrb;

        public uint sadrA
        {
            get
            {
                return endop && cmet ? (ushort)sCwA.qb
                    : push || endop ? cStk.qb + 1
                    : etime ? 0
                    : (cStk.qb == 0 ? 2
                    : cStk.qb);
            }
        }
        public uint sadrB
        {
            get
            {
                return etime ? endop ? call ? cStk.qb + 2
                    : 0
                    : cStk.qb + 1
                    : cStk.qb == 0 ? 0
                    : push ? cStk.qb + 2
                    : cStk.qb + 1;
            }
        }
        public uint nxtCw
        {
            get
            {
                return cStk.qb == 0 ? 2
                : cmet ? (ushort)sCwA.qb
                : cStk.qb + 1;
            }
        }
        //public int vadaa
        //{ get { return (pop ? (int)cStk.qa + 1 : pwaybit ? sCwA.qb & 0xffff : etime ? (int)cStk.qa : sCwA.qa >> 16); } }
        public int vadrA
        {
            get
            {
                return (int)(pop ? cStk.qa + 1
                : pway ? call ? cStk.qa : sCwA.qb & 0xffff
                : etime ? cStk.qa
                : sCwA.qa >> 16);
            }
        }
        //public int vadbb
        //{ get { return pop ? (int)cStk.qa : etime ? endop ? 0 : (int)sCwA.qb & 0xFFFF : (int)sCwA.qa & 0xFFFF; } }
        public int vadrB
        {
            get
            {
                return (int)(pop ? cStk.qa
                : (etime || call) && !endop ? (sCwA.qb & 0xFFFF)
                : (sCwA.qa & 0xFFFF));
            }
        }

        //int vac
        //{ get { return Parser.UcdEnum(sCwA.qb >> 16); } }  

        public bool wrtVa
        { get { return etime && !(pop || tjmp || fjmp) || pway; } }

        public bool etime
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.etime) > 0; } }
        public bool push
        { get { return ((sCwA.qb >> 16) & (uint)Parser.UcdEnum.push) > 0; } }
        public bool pop
        { get { return ((sCwA.qb >> 16) & (uint)Parser.UcdEnum.pop) > 0; } }
        public bool pway
        { get { return ((sCwA.qb >> 16) & (uint)Parser.UcdEnum.pway) > 0; } }
        public int fSel
        { get { return (int)((sCwA.qb >> 16) & 0xfff); } }
        public bool endop
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.eop) > 0; } }
        public bool call
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.call) > 0; } }
        public bool rtn
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.rtn) > 0; } }
        public bool less
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.less) > 0; } }
        public bool gtr
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.gtr) > 0; } }
        public bool eql
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.eql) > 0; } }
        public bool tjmp
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.tna) > 0; } }
        public bool fjmp
        { get { return ((sCwA.qb >> 16) & (int)Parser.UcdEnum.fna) > 0; } }
        //public bool lesscnd
        //{ get { return less & alu.alb > 0 ? true : false; } }
        //public bool gtrcnd
        //{ get { return gtr & alu.agb > 0 ? true : false; } }
        //public bool eqlcnd
        //{ get { return eql & alu.aeb > 0 ? true : false; } }
        public bool cmet
        {
            get
            {
                //return tjmp & !fjmp & (lesscnd | gtrcnd | eqlcnd) ? true 
                //    : (fjmp & !tjmp & (!lesscnd & !gtrcnd & !eqlcnd)) ? true 
                //    : tjmp & fjmp ? true : false;
                return false;
            }
        }
        //  10/5/13
        //_|c0|_______________|c0|________________|c0|_______________|c0|_______________|c0|___
        //          	      sCwA[2]                                eop ? sCwA[cwCt.qa + 1] : 0
        //					  sCwB[0]             sCwB[cwCt.qa + 1]  eop ? 0 : sCwB[cwCt.qa + 1] 
        //										  va[sCwA.qa]        pway ? pway@ : etime ? stkCt.qa]
        //										  vb[pop ? stkCt.qa : etime ? sCwB.qb : sCwA.qb]                            
        //                    cwCt.qa = 2         cwCtA = cwCt.qa + 1 
        // push cwCt and args, pop first two, wrt rslt & read nxt end with rslt in TOS.  return wrts TOS to pway loc, load cwCt
        // TOS can wrt toswitch (cbits)
        //{
        //    case "nop":
        //        sCwA.adra = cStk.qb;
        //        sCwA.cwAlways.Add(cbits + " : adra <= cStk.qb" );
        //        if (sCwA.adra != sadrA)
        //        { }
        //        sCwA.adrb = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adrb <= (cStk.qb + 1)");
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        vbls.adra = sCwA.qa >> 16 & 0xffff;
        //        vbls.cwAlways.Add(cbits + " : adra <= (sCwA.qa >> 16 + (cStk.qa & 0xffff000) >> 16)");
        //        if (vbls.adra != vadrA)
        //        { }
        //        vbls.adrb = (sCwA.qa & 0xFFFF) + ((cStk.qa & 0xffff000) >> 16);
        //        vbls.cwAlways.Add(cbits + " : adrb <=((sCwA.qa & 0xFFFF) + ((cStk.qa & 0xffff000) >> 16))");
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "etime, pway, eop":
        //        sCwA.adra = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adra <= cStk.qb");
        //        if (sCwA.adra != sadrA)
        //        { }
        //        sCwA.adrb = 0;
        //        sCwA.cwAlways.Add(cbits + " : adrb <= 0");
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        vbls.adra = sCwA.qb & 0xffff;
        //        vbls.cwAlways.Add(cbits + " : adra <= (sCwA.qb & 0xffff)");
        //        if (vadrA != (sCwA.qb & 0xffff) || sadrB != 0 || wrtVa != 1 || sadrA != cStk.qb + 1)
        //        { }
        //        vbls.adrb = 0;
        //        vbls.cwAlways.Add(cbits + " : adrb <= 0");
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "less, eop, tna":
        //    case "less, eop, fna":
        //    case "eql, eop, tna":
        //    case "eql, eop, fna":
        //    case "gtr, eop, tna":
        //    case "gtr, eop, fna":
        //        vbls.adra = cStk.qa;
        //        sCwA.adra = cmet ? (int)sCwA.qb & 0x0000FFFF : cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adra <= ( cmet ? sCwA.qb & 0x0000FFFF : cStk.qb + 1)");
        //        if (sCwA.adra != sadrA)
        //        { }
        //        //sCwA.adrb = cStk.qb + 1;
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        if (sCwA.adra != sadrA)
        //        { }
        //        vbls.adrb = 0;
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "pway, eop":
        //        oCwd = cbits + " " + aluCode;
        //        sCwA.adra = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adra <= (cStk.qb + 1).ToString()");
        //        if (sCwA.adra != sadrA)
        //        { }
        //        sCwA.adrb = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adrb <= (cStk.qb + 1).ToString()");
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        vbls.adra = sCwA.qb & 0xffff;
        //        vbls.cwAlways.Add(cbits + " : adra <= (sCwA.qb & 0xffff)");
        //        if (vbls.adra != vadrA)
        //        { }
        //        vbls.adrb = (int)(sCwA.qa & 0xFFFF) + ((cStk.qa & 0xffff000) >> 16);
        //        vbls.cwAlways.Add(cbits + " : adrb <= " + (sCwA.qa & 0xFFFF) + ((cStk.qa & 0xffff000) >> 16));
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "push":
        //        sCwA.adra = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adra <= (cStk.qb + 1)");
        //        if (sCwA.adra != sadrA)
        //        { }
        //        sCwA.adrb = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adrb <= (cStk.qb + 1)");
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        oCwd = cbits + " " + aluCode;
        //        vbls.adra = cStk.qa;
        //        if (vbls.adra != vadrA)
        //        { }
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "push, pway":
        //        vbls.adra = sCwA.qb & 0xffff;
        //        if (vbls.adra != vadrA)
        //        { }
        //        if (vbls.adrb != vadrB)
        //        { }
        //        if (sCwA.adra != sadrA)
        //        { }
        //        //sCwA.adrb = cStk.qb + 1;
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        break;
        //    case "pop":
        //        sCwA.adrb = cStk.qb + 1;
        //        sCwA.cwAlways.Add(cbits + " : adrb <= (cStk.qb + 1)");
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        if (sCwA.adra != sadrA)
        //        { }
        //        vbls.adra = cStk.qa + 1;
        //        if (vbls.adra != vadrA)
        //        { }
        //        vbls.adrb = (int)cStk.qa;
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "etime":
        //        //sCwA.adrb = cStk.qb + 1;
        //        if (sCwA.adra != sadrA)
        //        { }
        //        sCwA.adrb = cStk.qb + 1;
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        vbls.adra = cStk.qa;
        //        if (vbls.adra != vadrA)
        //        { }
        //        vbls.adrb = (int)sCwA.qb & 0xFFFF;
        //        if (vbls.adrb != vadrB)
        //        { }
        //        break;
        //    case "etime, push":
        //        vbls.adra = cStk.qa;
        //        oCwd = cbits + " " + aluCode;
        //        if (vadrA != cStk.qa)
        //        { }
        //        // sCwA.adrb = cStk.qb + 1;
        //        if (sCwA.adrb != sadrB)
        //        { }
        //        break;
        //    case "eop, less":

        //        break;
        //    case "eop, eql":

        //        break;
        //    case "eop, gtr":

        //        break;
        //    case "rtn":
        //        return;
        //        break;
        //    default:
        //        break;
        //}

        public class Alu
        {
            //    cvblsa, cvblsb, fSel, qa, alb, aeb, agb, stka, stkb, avbl, bstk
            // public Alu(MemCpu memCpu, AdrCtl adrCtl)
            //{
            //    ac = adrCtl;
            //    cpu = memCpu;
            //}
            public AdrCtl ac;
            public MemCpu cpu;
            public int alb { get { return (cpu.vRam.qa < cpu.vRam.qb) ? 1 : 0; } }
            public int aeb { get { return (cpu.vRam.qa == cpu.vRam.qb) ? 1 : 0; } }
            public int agb { get { return (cpu.vRam.qa > cpu.vRam.qb) ? 1 : 0; } }
            public int qa
            {
                get
                {

                    return 0;
                }
            }
        }
        //public class dff
        //{
        //    public dff(uint val)
        //    { myqa = val; }
        //    uint mydin = 0, myqa = 0;
        //    public uint din
        //    { set { mydin = value; } }
        //    public uint width
        //    { set { } }
        //    public uint qa
        //    { get { return myqa; } }
        //    public void clk()
        //    { myqa = mydin; }



        public class StkRam
        {
            public StkRam(uint[] ram)
            {
                sram = ram;
            }
            uint[] sram;
            uint inadra, inadrb, indina, indinb, adrA = 0, adrB = 0;
            bool cena = true, cenb = true;
            public bool wrena = true, wrenb = true;
            public bool clkena
            { set { cena = value; } }
            public bool clkenb
            { set { cenb = value; } }
            public uint dina
            { set { indina = value; } }
            public uint dinb
            { set { indinb = value; } }
            public uint adra
            { set { inadra = value; } }
            public uint adrb
            { set { inadrb = value; } }
            public uint qa
            { get { return sram[(int)adrA]; } }
            public uint qb
            { get { return sram[(int)adrB]; } }
            public void clka()
            { if (cena) adrA = inadra; if (wrena) sram[inadra] = indina; }
            public void clkb()
            { if (cenb) adrB = inadrb; if (wrenb) sram[inadrb] = indinb; }
        }


        public class CtlRom
        {
            public List<uint> uCWds = new List<uint>();
            uint inadra, inadrb, adrA = 0, adrB = 0;
            bool cena = true, cenb = true;
            public bool clkena
            { set { cena = value; } }
            public bool clkenb
            { set { cenb = value; } }
            public uint adra
            { set { inadra = value; } }
            public uint adrb
            { set { inadrb = value; } }
            public uint qa
            { get { return uCWds[(int)adrA]; } }
            public uint qb
            { get { return uCWds[(int)adrB]; } }
            public void clka()
            { if (cena) adrA = inadra; }
            public void clkb()
            { if (cenb) adrB = inadrb; }
        }

        public class ValRam
        {

            public List<int> vals = new List<int>();
            uint inadra, inadrb, adrA = 0, adrB = 0;
            int mydina, mydinb;
            bool mywrena, mywrenb;
            MemCpu.AdrCtl myac;
            public MemCpu.AdrCtl ac
            { get { return myac; } set { myac = value; } }
            public uint adra
            { get { return inadra; } set { inadra = value; } }
            public uint adrb
            { get { return inadrb; } set { inadrb = value; } }
            public int dina
            { set { mydina = value; } }
            public int dinb
            { set { mydinb = value; } }
            public bool wrena
            { set { mywrena = value; } }
            public bool wrenb
            { set { mywrenb = value; } }
            public int qa
            { get { return adrA < vals.Count ? vals[(int)adrA] : 0; } }
            public int qb
            { get { return adrB < vals.Count ? vals[(int)adrB] : 0; } }
            public void clka()
            { adrA = inadra; if (mywrena) vals[(int)adrA] = mydina; }
            public void clkb()
            {
                adrB = inadrb;
                if (mywrenb) vals[(int)adrB] = mydinb;
            }
        }


        char[] copers1 = new char[] { '+', '-', '!', '~', '=', '?', ':', '&', '|', '^', '*', '/', '<', '>', '%' };
        char[] wsp = new char[] { };


        public int datx(string dexp)
        {
            string[] bnms;
            Stack<int> dstk = new Stack<int>();
            int opix = 0;
            opix = dexp.IndexOfAny(copers1);
            bnms = dexp.Split(wsp, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in bnms)
            {
                if (char.IsLetterOrDigit(s[0]))
                {
                    if (s[0] == '~')
                    {
                        dstk.Push(~dstk.Pop());
                    }
                    else
                    {
                        // dstk.Push(do_op(s, dstk));
                    }
                }
            }
            return dstk.Pop();
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
            public MemCpu memCpu;
            //public Stack<SyntaxNodeOrToken> xStk = new Stack<SyntaxNodeOrToken>();
            //List<int> vtosv = new List<int>();
            XN xN;
            public List<XN> xNs = new List<XN>();
            XT xT;
            //public List<XT> xTs = new List<XT>();
            List<SyntaxNodeOrToken> ntl;
            //List<SyntaxNode> nL = new List<SyntaxNode>();
            //public Stack<XT> xTStk = new Stack<XT>();
            public Stack<XN> xNStk = new Stack<XN>();
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
                            xN = new XN(node);
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
                        if(char.IsDigit(xN.name[0]))
                        { }
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
                        xNStk.Push(new XN(node));
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
                        xN = new XN(node);
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


        public class XN
        {
            public XN(SyntaxNode sN)
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


        public class XT
        {
            public XT(SyntaxToken tkn)
            {
                this.token = tkn;
                this.node = tkn.Parent;
                if (char.IsDigit(tkn.ValueText[0]))
                {
                    vval = (int)tkn.Value;
                    asgnd = true;
                }
            }
            public string name
            {
                get { return token.ValueText; }
            }

            public virtual int val
            {
                get { return char.IsLetter(token.ValueText[0]) ? vval : (int)token.Value; }
                set
                {
                    vval = value;
                    asgnd = true;
                }
            }
            public SyntaxToken token;
            public SyntaxNode node;
            public int vval, scn_ix, vix = -1;
            public bool asgnd = false;
        }


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


        //[Flags]
        public enum RVenum
        {
            LUI = 0x00000037,
            AUIPC = 0X00000017,
            JAL = 0X0000006F,
            JALR = 0X00000067,
            BEQ = 0x00000063,
            BNE = 0x00001063,
            BLT = 0x00004063,
            BGE = 0x00005063,
            BLTU = 0x00006063,
            BGEU = 0x00007063,
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

        RVenum rVEnum = new RVenum();


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
            public String ckwd { get { return parent.kwd; } }
            public int lineno { get { return parent.srcln; } }

        }
    }
}
