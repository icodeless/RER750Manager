using System;
using System.Diagnostics;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750.Parameters
{
	public abstract class ParameterFormat
	{
		internal long _errorCode;

		public long ErrorCode => _errorCode;

		public abstract void ParseParameter(ref byte[] parameterByteArray);

		public abstract void ParseParameter(ref byte[] parameterByteArray, int startIndex, int length);

		internal void VerifyParameterLength(ref byte[] parameterByteArray, int startIndex, int length)
		{
			int verifyStep = 1;
			string errorMessage = "";
			bool exitLoop = false;
			checked
			{
				do
				{
					int num = verifyStep;
					if (num == 1)
					{
						if (startIndex >= parameterByteArray.Length)
						{
							errorMessage = "startIndex cannot exceed the parameterByteArray's length";
						}
					}
					else if (num == 2)
					{
						if (length == 0)
						{
							errorMessage = "length cannot be 0";
						}
					}
					else if (num == 3)
					{
						if (length > parameterByteArray.Length)
						{
							errorMessage = "length cannot exceed parameterByteArray's length";
						}
					}
					else if (num == 4)
					{
						if (startIndex + length > parameterByteArray.Length)
						{
							errorMessage = "startIndex+length cannnot large than the parameterByteArray's length";
						}
					}
					else if (num >= 5)
					{
						exitLoop = true;
					}
					else
					{
						Debugger.Break();
					}
					if (Operators.CompareString(errorMessage, "", TextCompare: false) != 0)
					{
						throw new Exception(errorMessage);
					}
					verifyStep++;
				}
				while (!exitLoop);
			}
		}
	}
}
