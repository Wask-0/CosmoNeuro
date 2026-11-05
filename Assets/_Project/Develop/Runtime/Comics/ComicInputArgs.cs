using _Project.Develop.Runtime.Utilities.SceneManagement;

namespace _Project.Develop.Runtime.Comics
{
    public sealed class ComicInputArgs : IInputSceneArgs
    {
        public ComicId Comic { get; }
        public string NextScene { get; }

        public ComicInputArgs(ComicId comic, string nextScene)
        {
            Comic = comic;
            NextScene = nextScene;
        }
    }
}
