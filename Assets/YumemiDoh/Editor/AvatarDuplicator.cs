using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace YumemiDoh.Editor
{
	public class AvatarDuplicator : EditorWindow
	{
		private GameObject _sourceAvatar;
		
		[MenuItem("AvatarDuplicator/Duplicator")]
		private static void ShowWindow()
		{
			var window = GetWindow<AvatarDuplicator>("AvatarDuplicator");
		}

		private void Check()
		{
			var avatarDuplicator = new AvatarDuplicatorCore(_sourceAvatar);

			if (avatarDuplicator.CheckDuplicateable())
			{
				Debug.Log("複製可能");
			}
			else
			{
				Debug.Log("複製不可");
			}
		}
		
		private void Duplicate()
		{
			var duplicator = new AvatarDuplicatorCore(_sourceAvatar);
			duplicator.DuplicateAvatar(_sourceAvatar.name + "_Duplicate");
		}
		
		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Source Object");
			_sourceAvatar = EditorGUILayout.ObjectField(_sourceAvatar, typeof(GameObject), true) as GameObject;
			GUILayout.EndHorizontal();

			EditorGUI.BeginDisabledGroup(!_sourceAvatar);
			if (GUILayout.Button("Check"))
			{
				Check();
			}
			if (GUILayout.Button("Duplicate"))
			{
				Duplicate();
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}