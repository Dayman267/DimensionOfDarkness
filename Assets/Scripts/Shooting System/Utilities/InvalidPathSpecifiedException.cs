using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvalidPathSpecifiedException : Exception
{
    public InvalidPathSpecifiedException(string AttributeName): base($"{AttributeName} does not exist at the provided path!")
    {
    }
}
