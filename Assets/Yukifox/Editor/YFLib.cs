using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.Windows;
using VRC.SDK3.Avatars.Components;

namespace Yukifox.Editor
{
	public static class YFLib
	{
		private static string _savePath = "Assets/Yukifox/Generated";

		public static string GetSavePath() => _savePath;

		public static void CreateAssetsFolder()
		{
			if (!Directory.Exists(_savePath))
			{
				AssetDatabase.CreateFolder("Assets/Yukifox", "Generated");
				AssetDatabase.Refresh();
			}
		}

		public static void CreateFolder(string name)
		{
			CreateAssetsFolder();
			if (!Directory.Exists(_savePath + "/" + name))
			{
				AssetDatabase.CreateFolder(_savePath, name);
				AssetDatabase.Refresh();
			}
		}

		public static void CreateFolderRecursive(string folderPath)
		{
			var arr = folderPath.Split('/');
			var path = "";
			var parentPath = _savePath;

			CreateAssetsFolder();
			foreach (var arrPath in arr)
			{
				if (!Directory.Exists(parentPath + "/" + arrPath))
				{
					AssetDatabase.CreateFolder(parentPath, arrPath);
					AssetDatabase.Refresh();
				}

				path += "/";
				parentPath += "/" + arrPath;
			}
		}

		public static AnimatorController GetController(this VRCAvatarDescriptor descriptor,
			VRCAvatarDescriptor.AnimLayerType type, out int index)
		{
			var layers = descriptor.baseAnimationLayers;
			index = -1;
			if (!descriptor.customizeAnimationLayers || layers.Length == 0)
				return null;

			for (var i = 0; i < layers.Length; i++)
			{
				if (layers[i].type == type)
				{
					index = i;

					if (layers[i].animatorController)
					{
						return layers[i].animatorController as AnimatorController;
					}
					else
					{
						return null;
					}
				}
			}

			return null;
		}
		
		public static readonly string[] IgnoreCopyFXLayers =
		{
			"AllParts",
			"Left Hand",
			"Right Hand"
		};
	}
}