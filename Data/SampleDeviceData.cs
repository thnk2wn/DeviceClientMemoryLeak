using System.Security.Cryptography;
using System.Text;

namespace RenewDeviceClientMemoryLeak.Data
{
    internal static class SampleDeviceData
    {
        private const string Sample = @"{""@t"":""2016-10-12T04:46:58.0554314Z"",""@mt"":""Hello, {@User}"",""User"":{""Name"":""nblumhardt"",""Id"":101}}
{""@t"":""2016-10-12T04:46:58.0684369Z"",""@mt"":""Number {N:x8}"",""@r"":[""0000002a""],""N"":42}
{""@t"":""2016-10-12T04:46:58.0724384Z"",""@mt"":""Tags are {Tags}"",""@l"":""Warning"",""Tags"":[""test"",""orange""]}
{""@t"":""2016-10-12T04:46:58.0904378Z"",""@mt"":""Something failed"",""@l"":""Error"", ""@x"":""System.DivideByZer...<snip>""}";

        public static byte[] GetBytes()
        {
            int repeat = RandomNumberGenerator.GetInt32(2, 150);
            var sb = new StringBuilder();

            for (var i = 0; i < repeat; i++)
            {
                sb.AppendLine(Sample);
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}
