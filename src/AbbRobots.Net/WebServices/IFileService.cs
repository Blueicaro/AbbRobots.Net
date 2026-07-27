
namespace AbbRobots.Net.WebServices;

public interface IFileService
{
  /// <summary>
  /// Get the content of the file.
  /// </summary>
   /// <param name="fileName">Name of the file with path</param>
  /// <returns>The contens of the file</returns>
  Task<List<string>> GetFileContent(string fileName);  
}