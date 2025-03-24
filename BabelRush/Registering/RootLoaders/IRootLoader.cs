namespace BabelRush.Registering.RootLoaders;

public interface IRootLoader
{
    public bool EnterDirectory(string dirName);
    public void LoadFile(string fileName, byte[] fileContent);

    /// <returns>true if the root directory was exited</returns>
    public  bool ExitDirectory();
}