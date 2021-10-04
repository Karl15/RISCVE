using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RISCVE
{
    public class RVE
    {
        public uint funct7, funct3, opcode, imm20, imm12;
        public int rs1, rs2, rd, iword;

        //enum Days { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday };
        //enum BoilingPoints { Celsius = 100, Fahrenheit = 212 };
        //[Flags]
        //public enum Colors { Red = 1, Green = 2, Blue = 4, Yellow = 8 };

        public enum FUNCodes
        {
            LUI = 0x37, AUIPC = 0X17, JAL = 0X6F,JALR = 0X67,
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
            FENCE = 0X0000000F, ECALL = 0X00000073, EBREAK = 0X00100073
        }

        //public enum opCodes { LUI  = 0x37,        AUIPC = 0X17,        JAL = 0X6F, 
        //               JALR = 0X67, 
        //               BEQ  = 0X00000063,  BNE  = 0X00004063,   BLT   = 0X00001063,
        //               BGE  = 0X00005063,  BLTU = 0X00006063,   BGEU  = 0X00007063,
        //               LB   = 0X00000003,  LH   = 0X00001063,   LW    = 0X00002063,
        //               LBU  = 0X00004003,  LHU  = 0X00005003,
        //               SB   = 0X00000023,  SH   = 0X00001023,   SW    = 0X00002023,
        //               ADDI = 0X00000023,  SLTI = 0X00002013,   SLTIU = 0X00003013,
        //               XORI = 0X00004013,  ORI  = 0X00006013,   ANDI  = 0X00007013,
        //               ADD  = 0X00000033,  SUB  = 0X40000033,   SLL   = 0X00001033,
        //               SLT  = 0X00002033,  SLTU = 0X00003033,   XOR   = 0X00004033,
        //               SRL  = 0X00005033,  SRA  = 0X40005033,   OR    = 0X00006033,
        //               AND  = 0X00007033,  
        //               FENCE= 0X0000000F, ECALL = 0X00000073,  EBREAK = 0X00100073
        //}


        //internal object GetType(RVE rVE)
        //{
        //    throw new NotImplementedException();
        //}

        public class RVtype
        {

            public class Rtype
            {
                Rtype(RVtype p)
                {
                    parent = p;
                }
                RVtype parent;
            }
            uint funct7, funct3, opcode;
            int rs1, rs2, rd;
        }

        public class Itype
        {
            Itype(RVtype p)
            {
                parent = p;
            }
            RVtype parent;
            uint funct7, funct3, opcode;
            int rs1, rd;
        }

        class Stype
        {
            Stype(RVtype p)
            {
                parent = p;
            }
            RVtype parent;
            uint imm7, funct3, imm5, opcode;
            int rs2, rs1;
        }
        class Btype
        {
            Btype(RVtype p)
            {
                parent = p;
            }
            RVtype parent;
            uint imm7, funct3, imm5, opcode;
            int rs2, rs1;
        }

        class Utype
        {
            Utype(RVtype p)
            {
                parent = p;
            }
            RVtype parent;
            uint imm7, funct3, imm5, opcode;
            int rs2, rs1;
        }

        class Jtype
        {
            Jtype(RVtype p)
            {
                parent = p;
            }
            RVtype parent;
            public uint imm20, opcode;
            int rd;
        }

        struct LUI
        {
            uint imm20, opcode;
            int rd;
        }
        public struct AUIPC
        {
            public uint imm20, opcode;
            public int rd;
        }
        public struct JAL
        {
            public uint imm20, opcode;
            public int rd;
        }
        public struct JALR
        {
            public uint imm20, opcode;
            public int rd;
        }
    }
}
