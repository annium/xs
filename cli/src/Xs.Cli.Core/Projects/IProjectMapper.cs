namespace Xs.Cli.Core.Projects
{
    public interface IProjectMapper<TProject, TRawProject>
    {
        TRawProject Load(string path);

        void Save(TProject project);
    }
}