using System.IO;
using System.Threading.Tasks;

namespace Xs.Cli.Core.Tools
{

    public class ShellAsyncResult
    {
        public StreamWriter Input { get; }

        public StreamReader Output { get; }

        public StreamReader Error { get; }

        public Task<ShellResult> Result { get; }

        public ShellAsyncResult(
            StreamWriter input,
            StreamReader output,
            StreamReader error,
            Task<ShellResult> result
        )
        {
            this.Input = input;
            this.Output = output;
            this.Error = error;
            this.Result = result;
        }
    }

    public class ShellResult
    {
        public int Code { get; }

        public string Output { get; }

        public string Error { get; }

        public ShellResult(int code, string output, string error)
        {
            this.Code = code;
            this.Output = output;
            this.Error = error;
        }

        public void Deconstruct(out int code, out string output, out string error)
        {
            code = Code;
            output = Output;
            error = Error;
        }
    }
}