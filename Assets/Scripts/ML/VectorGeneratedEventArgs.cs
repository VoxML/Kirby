using System;

public class VectorGeneratedEventArgs : EventArgs
{
    public float[] Vector;

    public VectorGeneratedEventArgs(float[] vector)
    {
        this.Vector = vector;
    }
}
