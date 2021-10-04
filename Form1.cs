using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;


namespace RISCV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        Parser parser = new Parser();
        RISCV rISCV = new RISCV();


        private void openFileDialog1_FileOk_1(object sender, CancelEventArgs e)
        {

            rISCV.Mem.iList.Add(0);
            //rISCV.Romar = 0;
            //Idcd[Romar] = 0;
            //opL = new XVal(node, 0);
            //ListBox lB1 = lB1in;
            //int lineno = 0;
            //StreamReader sR = new StreamReader(fPath);
            StreamReader sR = new StreamReader(@"C:\Users\Karl\Source\Demo100422020.cs");
            StringBuilder sB = new StringBuilder(sR.ReadToEnd()); // build tree from sR file
            //sB = new StringBuilder("main() { y = 2 + 3 + 4 - 5; x = y - 6; }");
            //sB = new StringBuilder("y = 2 + 3 + 4 - 5; x = y - 6;");
            CSharpSyntaxTree tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(sB.ToString());
            SyntaxNode node = tree.GetRoot();
            Parser. CEngineWalker cEwalker = new Parser.CEngineWalker();
            cEwalker.parser = parser;
            cEwalker.Visit(node);
            Console.WriteLine(tree);
            return;
            //decode();
            //riscv.Build(ref riscv, openFileDialog1.FileName, listBox1, parser);
            //riscv.decode(ref riscv);
            //int i = 0x1000;
            //int j = 0x2000;
            //int k = i - j;
            //int status;
            //List<int> l = new List<int>();
            //l.Add(1);
            //l.Add(2);
            //l.Add(3);
            //k = l[1];
            //l.RemoveAt(1);

            //int op1, op2, a, b, rslt;
            //a = 0x44;
            //b = 0x4;
            //if (a < 0)
            //    op1 = ~a + 1;
            //if (b < 0)
            //    op2 = ~b + 1; 
            //rslt = a * b;
            // 0x100
            // 0x010
            // 0x01
            // 0x100
            // 0x1000
            //rslt = alu.mpy(a, b);


            //if (j > i)
            //    k &= 0x000Fffff;
            //rISCV.push();
            //riscA.Run();
            //string[] names = Enum.GetNames(typeof(RVE));
            //Console.WriteLine("Members of {0}:", typeof(RVE).Name);
            //Array.Sort(names);
            //foreach (var name in names)
            //{
            //    //status = rVE.GetType(rVE).Parse(typeof(RVE), name);
            //    //Console.WriteLine("   {0} ({0:D})", status);
            //}
            //Console.WriteLine(
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            //StringBuilder sB = new StringBuilder();
            //sB.Append(0x33.ToString());
            //sB.Append(0xd.ToString());
            //sB.Append(0x33.ToString());
            //sB.Append(0xd.ToString());
            //Char[] carray = sB.ToString().ToCharArray();
            //utp = uint.TryParse(carray.ToString(), out ui);
            //Array.Reverse(carray);
            
            //utp =  uint.TryParse(carray.ToString(), out ui);
            openFileDialog1.ShowDialog();
        }
    }
}
