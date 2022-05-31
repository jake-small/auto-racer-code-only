public interface DataLoader
{
  T LoadJsonData<T>(string jsonFile);
  bool CanLoadResource();
  T LoadResourceData<T>(string resourceFile);
}