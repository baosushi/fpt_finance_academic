using FuGradeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FGReader
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream fileStream = new FileStream(@"C:\Users\Temporary\Desktop\phuonglhk.fg", FileMode.Open);
            var gradeFile = (TeacherGrade)new BinaryFormatter
            {
                AssemblyFormat = FormatterAssemblyStyle.Simple
            }.Deserialize(fileStream);
            fileStream.Close();


        }
    }
}
