using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace STPC.DynamicForms.Web.RT.Helpers
{
    public class GenerarPassword
    {
        public GenerarPassword()
        {

        }

        public int numeros { get; set; }
        public int mayus { get; set; }
        public int minus { get; set; }
        public int especial { get; set; }
        private Random randomltM = new Random(Guid.NewGuid().GetHashCode());

        public string generado(string posibles, int cantidad)
        {
            var ltM = posibles;
            var resultltM = new string(
                Enumerable.Repeat(ltM, cantidad)
                          .Select(s => s[randomltM.Next(s.Length)])
                          .ToArray());
            return resultltM;
        }

        private string randomic(string cadena)
        {
            string[] arra = cadena.Select(c => c.ToString()).ToArray();
            string[] MyRandomArray = arra.OrderBy(x => randomltM.Next()).ToArray();
            var Pwd = new StringBuilder();
            for (int i = 0; i < MyRandomArray.Length; i++)
                Pwd.Append(MyRandomArray[i]);
            return Pwd.ToString();
        }

        public string creapwd()
        {

            string StrMayus, StrMinus, StrNumer, StrEspec;
            StrMayus = generado("ABCDEFGHIJKLMNOPQRSTUVWXYZ", mayus);
            StrMinus = generado("abcdefghijklmnopqrstuvwxyz", minus);
            StrNumer = generado("0123456789", numeros);
            StrEspec = generado("][?/#!@$%^&*()+=}|{}", especial);
            string a = string.Format("{0}{1}{2}{3}", StrMayus, StrMinus, StrNumer, StrEspec);
            string PwdT = randomic(a);
            return PwdT;

        }
    }
}