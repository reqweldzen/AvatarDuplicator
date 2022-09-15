using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.VersionControl;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace YumemiDoh.Editor
{
	public class AvatarDuplicatorCore
	{
		private const string RootPath = "Assets/YumemiDoh/Generated";
		
		private readonly GameObject _sourceAvatar;

		public AvatarDuplicatorCore(GameObject sourceAvatar)
		{
			_sourceAvatar = sourceAvatar;
		}

		public bool CheckDuplicateable()
		{
			if (!CheckAvatar()) return false;

			return true;
		}

		private bool CheckAvatar()
		{
			var avatarAnimator = _sourceAvatar.GetComponent<Animator>();
			if (!avatarAnimator)
			{
				Debug.Log("モデルデータではありません。");
				return false;
			}
			
			var avatarDescriptor = _sourceAvatar.GetComponent<VRCAvatarDescriptor>();
			if (!avatarDescriptor)
			{
				Debug.Log("VRCアバターではありません。");
				return false;
			}
			
			var pipelineManager = _sourceAvatar.GetComponent<PipelineManager>();
			if (!pipelineManager)
			{
				Debug.Log("VRCアバターではありません。");
				return false;
			}

			return true;
		}
		
		public void DuplicateAvatar(string sourceAvatarName)
		{
			if (!CheckDuplicateable()) return;
			
			var duplicatedAvatar = Object.Instantiate(_sourceAvatar, Vector3.zero, Quaternion.identity);
			duplicatedAvatar.name = sourceAvatarName;

			SaveDuplicateAvatar(duplicatedAvatar);
		}
		
		private void SaveDuplicateAvatar(GameObject avatar)
		{
			if (!Directory.Exists(RootPath))
			{
				AssetDatabase.CreateFolder("Assets/YumemiDoh", "Generated");
			}

			var avatarName = avatar.name;

			string rootDir = null;
			if (!Directory.Exists($"{RootPath}/{avatarName}"))
			{
				AssetDatabase.CreateFolder(RootPath, avatarName);
				rootDir = $"{RootPath}/{avatarName}";
			}
			else
			{
				for (var i = 1; i < ushort.MaxValue; i++)
				{
					var extraPath = $"{RootPath}/{avatarName}_{i}";
					if (Directory.Exists(extraPath))
						continue;
					
					AssetDatabase.CreateFolder(RootPath, $"{avatarName}_{i}");
					rootDir = extraPath;
					break;
				}

				if (!Directory.Exists(rootDir))
				{
					Debug.Log("作成できる連番の制限を超えています。");
					Object.DestroyImmediate(avatar);
					return;
				}
			}
			
			var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
			
			var fx = descriptor.baseAnimationLayers[4];
			var fxPath = AssetDatabase.GetAssetPath(fx.animatorController);
			
			var exMenu = descriptor.expressionsMenu;
			var exMenuPath = AssetDatabase.GetAssetPath(exMenu);
			// TODO: Copy SubMenu
			var exParam = descriptor.expressionParameters;
			var exParamPath = AssetDatabase.GetAssetPath(exParam);

			if (!string.IsNullOrEmpty(fxPath))
			{
				var newPath = $"{rootDir}/{fx.animatorController.name}.controller";
				if (AssetDatabase.CopyAsset(fxPath, newPath))
				{
					var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(newPath);
					descriptor.baseAnimationLayers[4].animatorController = controller;
				}
			}

			if (!string.IsNullOrEmpty(exMenuPath))
			{
				var newPath = $"{rootDir}/{exMenu.name}.asset";
				if (AssetDatabase.CopyAsset(exMenuPath, newPath))
				{
					descriptor.expressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(newPath);
				}
			}
			
			if (!string.IsNullOrEmpty(exParamPath))
			{
				var newPath = $"{rootDir}/{exParam.name}.asset";
				if (AssetDatabase.CopyAsset(exParamPath, newPath))
				{
					descriptor.expressionParameters = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(newPath);
				}
			}

			var pipelineManager = avatar.GetComponent<PipelineManager>();
			Object.DestroyImmediate(pipelineManager);
			avatar.AddComponent<PipelineManager>();


			var localPath = $"{rootDir}/{avatarName}.prefab";

			localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

			bool prefabSuccess;
			PrefabUtility.SaveAsPrefabAssetAndConnect(avatar, localPath, InteractionMode.UserAction,
				out prefabSuccess);

			if (prefabSuccess)
			{
				Debug.Log("Prefab was saved successfully");
				Object.DestroyImmediate(avatar);
			}
			else
			{
				Debug.Log("Prefab failed to save.");
				Object.DestroyImmediate(avatar);
			}
		}
	}
}