using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSrcAST;
using System.Windows.Forms;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace OpenSrcAST
    {
        public class MemCpu
        {
            //public MemCpu()
            //{
            //    Parser vP = new Parser(this);
            //    alu = new Alu(this, aC);
            //}

            public List<string> lB1 = new List<string>();
            //List<string> cwOps = new List<string>();
            public List<Parser.VNode> cVs = new List<Parser.VNode>();
            public StkRam cStk = new StkRam(new uint[64]);
            public VRam vRam;
            public CtlRom sCwA;
            public Alu alu;
            AdrCtl aC = new AdrCtl();


            //string[] ixf1, ixf2;
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
            public void ipl(System.Windows.Forms.ListBox lBin, List<uint> mem, List<Parser.VNode> cVin)
            {
                AdrCtl aC = new AdrCtl();
                vRam = new VRam();
                foreach (Parser.VNode v in cVin)
                    vRam.vbls.Add(v.val);
                //cV = cVin;
                mem.Add(0);
                sCwA = new CtlRom(mem);
                sCwA.srom.Capacity = 0x40;
                sCwA.srom.AddRange(mem.ToArray());

                sCwA.srom.Add(0);
                sCwA.srom.Add(0);

                //vbls.Capacity = 256;
                //while vbls.Count < vbls.Capacity)
                //    vbls.Add(0);

                //      Stack<int> iStk = new Stack<int>(aC.vbls.vbls.ToArray());
                //Alu.ac = aC;
                //ftch ocw, op2@, op1@;  load ocwReg, nxt@; ftch op1,op2, wrt TOS, fth ocw, op2@;  rd nxtOp, TOS



                lBin.Items.Add("Starting");

                //cVin.Reverse();
                //cVin.Add(cVin[4]);
                pec0();
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
                    if (!Int32.TryParse(src.Substring(2), NumberStyles.HexNumber, null, out xval))
                    {
                        MessageBox.Show("num Parse error");
                        return false;
                    }
                }
                else
                {
                    if (!Int32.TryParse(src, out xval))
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
            vadra = (pop ? (cStk.qa + 1) : 0)
                    | ((etime && !pway) || (pway && call) ? cStk.qa : 0)
                    | ((pway && !call) ? sCwA.qb & 0xffff : 0)
                    | ((!etime && !pway && !call) ? sCwA.qa >> 16 : 0);
            vadrb = (UInt16)((pop ? cStk.qa : 0)
                    | ((etime || call) && !endop ? sCwA.qb : 0)
                    | ((etime || call) && endop ? sCwA.qa : 0));
            sadra = (UInt16)((!etime && !eop ? (cStk.qb == 0 ? 2 : cStk.qb) : 0)
                    | ((etime && (cmet || call) ? sCwA.qb : 0)
                    | ((endop || push) ? (cStk.qb + 1) : 0)
                    | (endop ? cStk.qb + 1 : 0)));
            //sadra = (UInt16)((!etime && !eop ? cStk.qb == 0 ? 2 : cStk.qb : 0)
            //        | (push ? cStk.qb + 1 : 0)
            //        | ((etime && (endop || push)) ? cmet || call ? (UInt16)sCwA.qb : (UInt16)(cStk.qb + 1) : 0)
            //        | (!etime && endop ? cStk.qb + 1 : 0));
            sadrb = (UInt16)((etime && (endop || push)) ? 0 : cStk.qb + 1);

        }

        public void pec0()
        {
            cStk.adra = 0;
                cStk.wrena = true;
                cStk.adrb = 1;
                cStk.wrenb = true;
                cStk.dina = ((uint)vRam.vbls.Capacity - 1);
                cStk.dinb = 0;
                cStk.clka();
                cStk.clkb();


                int cycle = 0;
                uint callPtr = 0;
                //string fmt;
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
                    sadra = (UInt16)(!etime && !endop ? (cStk.qb & 0xffff) == 0 ? 2 : cStk.qb + 1
                        : etime ? (cmet || call && endop && !rtn) ? sCwA.qb
                        : (endop || push) ? call ? 0 : (cStk.qb + 1) : 0 : cStk.qb + 1);
                    sadrb = (UInt16)(etime && endop || push && !call || !etime && pway ? 0 : cStk.qb + 1);


                    //fmt = String.Format("cycle = {0} {1}{2}{3}{4}{5}{6} Op1 = {7}; {8} Op2 = {9}; Alu = {10} {11} {12}"
                    // , cycle, (Parser.ucdEnum)(sCwA.qb & 0xfff0), (tjmp ? ",tna " : ""), (fjmp ? ",fna " : "") // 0, 1, 2, 3
                    // , (gtr ? ",gtr " : ""), (eql ? ",eql " : ""), (less ? ",less" : "") // 4, 5, 6
                    // , vbls.qa, (Parser.ucdEnum)((sCwA.qb >> 16) & 0x01f), vbls.qb, Alu.qa// 7, 8, 9
                    // , wrtVa ? (("; wrt " + Alu.qa + " to ") + ((pway && !call) ? (int)vadra < cV.Count ? cV[(int)vadra].name : "vbls[" + vadra + "]" : "TOS")) : ""
                    // //                                                                  ----------------------------------------------------
                    // //                                          --------------------------------------------------------------------------- 
                    // // ------------------------------------------------------------------------------------------------------------------------
                    // , cmet ? " cmet " : ""); //
                    //lB1.Add(fmt);
                    if (lB1.Count >= 100)
                        return;
                    //c0  
                    sCwA.adra = (UInt16)sadra;   // (((UInt16)cStk.qb) == 0 ? 2 : cStk.qb + 1);
                    sCwA.adrb = (UInt16)sadrb;  // (endop ? 0 : cStk.qb + 1);
                    AdrCtl myac = new AdrCtl();
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
                        : call ? push ? (uint)(((callPtr & 0xffff) + 2) << 16) | (UInt16)(cStk.qa - 1)
                        : rtn ? (uint)(((callPtr & 0xffff) - 2) << 16) | (UInt16)(cStk.qa - 1)
                        : (uint)((callPtr & 0xffff) << 16) | (UInt16)(cStk.qa - 1)
                        : push ? cStk.qa - 1 : cStk.qa;
                    cStk.dinb = call && endop && !rtn ? cStk.qa << 16 | (UInt16)sCwA.qb : cStk.qb & 0xffff0000
                        | (UInt16)((cmet || call && endop && !rtn) ? sCwA.qb
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
                    vRam.adra = (UInt16)vadra;
                    vRam.adrb = (UInt16)vadrb;
                    vRam.wrena = wrtVa ? true : false;
                    if (wrtVa)
                    { }
                    sCwA.clka();
                    sCwA.clkb();
                    vRam.clka();
                    vRam.clkb();
                    if (sCwA.qa == 0xffffffff || sCwA.qb == 0xffffffff)
                        return;
                    cStk.clka();
                    cStk.clkb();
                    cycle++;
                    uint dcd = (sCwA.qb >> 16);
                    //   sCwA.adra = 0; sCwA.adrb = 0; vbls.adra = 0; vbls.adrb = 0;
                    String cbits = ((Parser.ucdEnum)(dcd & 0XFFF0)).ToString(), aluCode;  //, sCwd, oCwd;

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

                    aluCode = ((Parser.ucdEnum)((sCwA.qb >> 16) & 0X1F)).ToString();

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
                    return endop && cmet ? (UInt16)sCwA.qb
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
                    : cmet ? (UInt16)sCwA.qb
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

            public bool wrtVa
            { get { return etime && !(pop || tjmp || fjmp) || pway; } }

            public bool etime
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.etime) == (uint)Parser.ucdEnum.etime ? true : false; } }
            public bool push
            { get { return ((sCwA.qb >> 16) & (uint)Parser.ucdEnum.push) == (uint)Parser.ucdEnum.push ? true : false; } }
            public bool pop
            { get { return ((sCwA.qb >> 16) & (uint)Parser.ucdEnum.pop) == (uint)Parser.ucdEnum.pop ? true : false; } }
            public bool pway
            { get { return ((sCwA.qb >> 16) & (uint)Parser.ucdEnum.pway) == (uint)Parser.ucdEnum.pway ? true : false; } }
            public int fSel
            { get { return (int)((sCwA.qb >> 16) & 0xfff); } }
            public bool endop
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.eop) == (int)Parser.ucdEnum.eop ? true : false; } }
            public bool call
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.call) == (int)Parser.ucdEnum.call ? true : false; } }
            public bool rtn
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.rtn) == (int)Parser.ucdEnum.rtn ? true : false; } }
            public bool less
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.less) == (int)Parser.ucdEnum.less ? true : false; } }
            public bool gtr
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.gtr) == (int)Parser.ucdEnum.gtr ? true : false; } }
            public bool eql
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.eql) == (int)Parser.ucdEnum.eql ? true : false; } }
            public bool tjmp
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.tna) == (int)Parser.ucdEnum.tna ? true : false; } }
            public bool fjmp
            { get { return ((sCwA.qb >> 16) & (int)Parser.ucdEnum.fna) == (int)Parser.ucdEnum.fna ? true : false; } }
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
            // TOS can wrt to
            // switch (cbits)
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
                public Alu(MemCpu memCpu, AdrCtl adrCtl)
                {
                    ac = adrCtl;
                    cpu = memCpu;
                }
                public AdrCtl ac;
                MemCpu cpu;
                public int alb { get { return (cpu.vRam.qa < cpu.vRam.qb) ? 1 : 0; } }
                public int aeb { get { return (cpu.vRam.qa == cpu.vRam.qb) ? 1 : 0; } }
                public int agb { get { return (cpu.vRam.qa > cpu.vRam.qb) ? 1 : 0; } }
            }
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
                public CtlRom(List<uint> rom)
                {
                    srom = new List<uint>(rom);
                }
                public List<uint> srom = new List<uint>();
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
                { get { return srom[(int)adrA]; } }
                public uint qb
                { get { return srom[(int)adrB]; } }
                public void clka()
                { if (cena) adrA = inadra; }
                public void clkb()
                { if (cenb) adrB = inadrb; }
            }

            public class VRam
            {
                public VRam()
                {
                    vbls = new List<int>();
                }
                public List<int> vbls;
                uint inadra, inadrb, adrA = 0, adrB = 0;
                int mydina, mydinb;
                bool mywrena, mywrenb;
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
                { get { return adrA < vbls.Count ? vbls[(int)adrA] : 0; } }
                public int qb
                { get { return adrB < vbls.Count ? vbls[(int)adrB] : 0; } }
                public void clka()
                { adrA = inadra; if (mywrena) vbls[(int)adrA] = mydina; }
                public void clkb()
                {
                    adrB = inadrb;
                    if (mywrenb) vbls[(int)adrB] = mydinb;
                }
            }


        }
    }
//switch (cbits)
//{
//    case "nop":
//        sCwA.adra = cStk.qb;
//        sCwA.cwAlways.Add(cbits + " : adra <= cStk.qb");
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
////        if (vbls.adrb != vadrB)
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
    //    vbls.cwAlways.Add(cbits + " : adrb <= " + (sCwA.qa & 0xFFFF) + ((cStk.qa & 0xffff000) >> 16));
    //    if (vbls.adrb != vadrB)
    //    { }
    //    break;
    //case "push":
    //    sCwA.adra = cStk.qb + 1;
    //    sCwA.cwAlways.Add(cbits + " : adra <= (cStk.qb + 1)");
    //    if (sCwA.adra != sadrA)
    //    { }
    //    sCwA.adrb = cStk.qb + 1;
    //    sCwA.cwAlways.Add(cbits + " : adrb <= (cStk.qb + 1)");
    //    if (sCwA.adrb != sadrB)
    //    { }
    //    oCwd = cbits + " " + aluCode;
    //    vbls.adra = cStk.qa;
    //    if (vbls.adra != vadrA)
    //    { }
    //    if (vbls.adrb != vadrB)
    //    { }
    //    break;
    //case "push, pway":
    //    vbls.adra = sCwA.qb & 0xffff;
    //    if (vbls.adra != vadrA)
    //    { }
    //    if (vbls.adrb != vadrB)
    //    { }
    //    if (sCwA.adra != sadrA)
    //    { }
    //    //sCwA.adrb = cStk.qb + 1;
    //    if (sCwA.adrb != sadrB)
    //    { }
    //    break;
    //case "pop":
    //    sCwA.adrb = cStk.qb + 1;
    //    sCwA.cwAlways.Add(cbits + " : adrb <= (cStk.qb + 1)");
    //    if (sCwA.adrb != sadrB)
    //    { }
    //    if (sCwA.adra != sadrA)
    //    { }
    //    vbls.adra = cStk.qa + 1;
    //    if (vbls.adra != vadrA)
    //    { }
    //    vbls.adrb = (int)cStk.qa;
    //    if (vbls.adrb != vadrB)
    //    { }
    //    break;
    //case "etime":
    //    //sCwA.adrb = cStk.qb + 1;
    //    if (sCwA.adra != sadrA)
    //    { }
    //    sCwA.adrb = cStk.qb + 1;
    //    if (sCwA.adrb != sadrB)
    //    { }
    //    vbls.adra = cStk.qa;
    //    if (vbls.adra != vadrA)
    //    { }
    //    vbls.adrb = (int)sCwA.qb & 0xFFFF;
    //    if (vbls.adrb != vadrB)
    //    { }
    //    break;
    //case "etime, push":
    //    vbls.adra = cStk.qa;
    //    oCwd = cbits + " " + aluCode;
    //    if (vadrA != cStk.qa)
    //    { }
    //    // sCwA.adrb = cStk.qb + 1;
    //    if (sCwA.adrb != sadrB)
    //    { }
    //    break;
    //case "eop, less":

    //    break;
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
