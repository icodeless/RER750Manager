using System;

namespace GIGATMS.Protocol
{
	public interface IPackage
	{
		bool bReceived { get; }

		bool bCheckSum { get; }

		bool IsFullSize { get; }

		byte DataLength { get; }

		byte[] Datas { get; set; }

		void ClearPackage();

		byte[] getBuffer();

		bool getData(ref string Value);

		bool getData(ref bool Value);

		bool getData(ref byte Value);

		bool getData(ref short Value, bool bIsReverse = true);

		bool getData(ref int Value, bool bIsReverse = true);

		bool getData(ref long Value, bool bIsReverse = true);

		bool getData(ref DateTime Value);

		bool getData(ref byte[] Value);

		bool PackageAppend(ref byte Value);

		bool PackageFill(ref byte[] Value, bool isProveState = false);

		string getItemName(int Value);

		byte[] getItemValue(int Index);
	}
}
