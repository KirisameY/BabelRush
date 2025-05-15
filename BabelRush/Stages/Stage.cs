using System;

using BabelRush.Scenery;

namespace BabelRush.Stages;

public class Stage : IDisposable
{
    #region Dispose

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;
        //todo
    }

    #endregion

    public Scene CreateScene()
    {
        return new();
    }
}