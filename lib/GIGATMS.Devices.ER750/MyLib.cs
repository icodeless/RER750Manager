using System;
using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace GIGATMS.Devices.ER750
{
	internal class MyLib
	{
		public class Encoding
		{
			public static string LongHex(long Number)
			{
				string h = "";
				checked
				{
					do
					{
						long i = (long)Math.Truncate((double)Number / 16.0);
						long r = Number - i * 16;
						h = Convert.ToString(Convert.ToInt32(r), 16).ToUpper() + h;
						Number = i;
					}
					while (Number > 0);
					return h;
				}
			}

			public static bool NumberToByteArray(long Number, ref byte[] byteArray, int startIndex, int Length)
			{
				if (Length >= 1 && Length < 5)
				{
					checked
					{
						string tempHex = "00000000" + LongHex(Number);
							string numberHex = tempHex.Substring(Math.Max(0, tempHex.Length - 2 * Length));
						int hexLength = numberHex.Length;
						int num = hexLength;
						for (int i = 1; i <= num; i += 2)
						{
							byteArray[(int)Math.Round((double)startIndex + (double)(i - 1) / 2.0)] = (byte)Math.Round(Convert.ToDouble(Convert.ToInt32(numberHex.Substring(hexLength - i - 1, 2), 16)));
						}
						return true;
					}
				}
				Debugger.Break();
				return false;
			}

			public static object GetString(byte[] byteArray)
			{
				return GetString(byteArray, 0, byteArray.Length);
			}

			public static object GetString(byte[] byteArray, int startIndex, int length)
			{
				string dataString = "";
				checked
				{
					try
					{
						int endIndex = startIndex + length - 1;
						int num = endIndex;
						for (int i = startIndex; i <= num && ((byteArray[i] >= 20) & (byteArray[i] <= 127)); i++)
						{
							dataString += Conversions.ToString(Convert.ToChar(byteArray[i]));
						}
						return dataString;
					}
					catch (Exception ex)
					{
						ProjectData.SetProjectError(ex);
						Exception ex2 = ex;
						throw new Exception(ex2.Message);
					}
				}
			}

			public static byte[] GetBytes(string stringData)
			{
				return System.Text.Encoding.Default.GetBytes(stringData);
			}

			public static object ByteArrayToSignedNumber(ref byte[] byteArray)
			{
				long convertedNumber = 0L;
				long negativeNumber = 0L;
				long tmp = 0L;
				convertedNumber = 0L;
				if (byteArray.Length > 8)
				{
					throw new Exception("Byte array cannot exceed than 8");
				}
				checked
				{
					int num = byteArray.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						tmp = byteArray[i];
						tmp <<= 8 * i;
						convertedNumber += tmp;
					}
					switch (byteArray.Length)
					{
					case 1:
						if (convertedNumber > 127)
						{
							negativeNumber = 256L;
							convertedNumber = (sbyte)(-(negativeNumber - convertedNumber));
						}
						else
						{
							convertedNumber = (sbyte)convertedNumber;
						}
						break;
					case 2:
						if (convertedNumber > 32767)
						{
							negativeNumber = 65536L;
							convertedNumber = (short)(-(negativeNumber - convertedNumber));
						}
						else
						{
							convertedNumber = (short)convertedNumber;
						}
						break;
					case 4:
						if (convertedNumber > int.MaxValue)
						{
							negativeNumber = 4294967296L;
							convertedNumber = (int)(-(negativeNumber - convertedNumber));
						}
						else
						{
							convertedNumber = (int)convertedNumber;
						}
						break;
					default:
						Debugger.Break();
						convertedNumber = convertedNumber;
						break;
					}
					return convertedNumber;
				}
			}

			public static sbyte ByteToSByte(byte byteValue)
			{
				byte[] b = new byte[1] { byteValue };
				return Conversions.ToSByte(ByteArrayToSignedNumber(ref b));
			}

			public static object ByteArrayToSignedNumber(ref byte[] byteArray, int startIndex, int length)
			{
				byte[] byteArrayX = new byte[checked(length - 1 + 1)];
				long convertedNumber = 0L;
				Array.Copy(byteArray, startIndex, byteArrayX, 0, length);
				convertedNumber = Conversions.ToLong(ByteArrayToSignedNumber(ref byteArrayX));
				return convertedNumber;
			}

			public static object ByteArrayToUnsignedNumber(ref byte[] byteArray)
			{
				ulong convertedNumber = 0uL;
				ulong negativeNumber = 0uL;
				ulong tmp = 0uL;
				convertedNumber = 0uL;
				if (byteArray.Length > 8)
				{
					throw new Exception("Byte array cannot exceed than 8");
				}
				checked
				{
					int num = byteArray.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						tmp = byteArray[i];
						tmp <<= 8 * i;
						convertedNumber += tmp;
					}
					switch (byteArray.Length)
					{
					case 1:
						convertedNumber = (byte)convertedNumber;
						break;
					case 2:
						convertedNumber = (ushort)convertedNumber;
						break;
					case 4:
						convertedNumber = (uint)convertedNumber;
						break;
					default:
						Debugger.Break();
						convertedNumber = convertedNumber;
						break;
					}
					return convertedNumber;
				}
			}

			public static object ByteArrayToUnsignedNumber(ref byte[] byteArray, int startIndex, int length)
			{
				byte[] byteArrayX = new byte[checked(length - 1 + 1)];
				ulong convertedNumber = 0uL;
				Array.Copy(byteArray, startIndex, byteArrayX, 0, length);
				convertedNumber = Conversions.ToULong(ByteArrayToUnsignedNumber(ref byteArrayX));
				return convertedNumber;
			}

			public static string ByteArrayToHexString(byte[] byteArray)
			{
				string hexString = "";
				checked
				{
					int num = byteArray.Length - 1;
					for (int i = 0; i <= num; i++)
					{
						hexString += Convert.ToString(Convert.ToInt32(byteArray[i]), 16).ToUpper().PadLeft(2, '0');
					}
					return hexString;
				}
			}
		}
	}
}
