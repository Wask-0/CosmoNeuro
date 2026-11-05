using System;

namespace _Project.Develop.Runtime.Utilities.Foundation.Text
{
    public interface ITextLocalizer
    {
        event Action Changed;
        string Get(string area, string source);
    }
}
