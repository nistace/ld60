using System;
using System.Collections.Generic;
using System.Linq;
using LD60.Railway;
using LD60.Railway.Renderers;
using LD60.Trains;
using UnityEditor;
using UnityEngine;

namespace LD60.Levels.Editor
{
    [CustomEditor(typeof(Level))]
    public class LevelCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Build railway"))
            {
                BuildRailway();
            }
        }

        private void BuildRailway()
        {
            var level = (Level)target;
            var chunks = level.GetComponentsInChildren<RailwayChunk>();

            var minX = chunks.Select(t => t.transform.localPosition.x).Min();
            var minY = chunks.Select(t => t.transform.localPosition.y).Min();

            foreach (var chunk in chunks)
            {
                chunk.name = $"Rail [{-minX + chunk.transform.localPosition.x:00}{-minY + chunk.transform.localPosition.y:00}]";
                if (!TryAssignBestNextChunkBackwardMatch(chunk, chunks))
                {
                    chunk.NextChunkBackward = new RailwayChunkWithDirection();
                    Debug.LogWarning($"No origin found for {chunk.name}");
                }

                if (!TryAssignBestNextChunkForwardMath(chunk, chunks))
                {
                    chunk.NextChunkForward = new RailwayChunkWithDirection();
                    Debug.LogWarning($"No destination found for {chunk.name}");
                }

                EditorUtility.SetDirty(chunk);
            }

            foreach (var lightComponent in level.GetComponentsInChildren<LightRailwayComponent>())
            {
                var lightSo = new SerializedObject(lightComponent);
                lightSo.Update();

                var chunksProperty = lightSo.FindProperty("_chunks");

                chunksProperty.arraySize = 0;

                foreach (var chunk in chunks)
                {
                    if (chunk.WorldOrigin == lightComponent.transform.position)
                    {
                        chunksProperty.InsertArrayElementAtIndex(chunksProperty.arraySize);
                        chunksProperty.GetArrayElementAtIndex(chunksProperty.arraySize - 1).FindPropertyRelative("_chunk").objectReferenceValue = chunk;
                        chunksProperty.GetArrayElementAtIndex(chunksProperty.arraySize - 1).FindPropertyRelative("_direction").enumValueIndex = (int)RailwayChunkWithDirection.EDirection.Forward;
                    }

                    if (chunk.WorldDestination == lightComponent.transform.position)
                    {
                        chunksProperty.InsertArrayElementAtIndex(chunksProperty.arraySize);
                        chunksProperty.GetArrayElementAtIndex(chunksProperty.arraySize - 1).FindPropertyRelative("_chunk").objectReferenceValue = chunk;
                        chunksProperty.GetArrayElementAtIndex(chunksProperty.arraySize - 1).FindPropertyRelative("_direction").enumValueIndex = (int)RailwayChunkWithDirection.EDirection.Backward;
                    }
                }

                lightSo.ApplyModifiedProperties();
                lightComponent.name = $"Light {lightComponent.NameOnPanel}";
                EditorUtility.SetDirty(lightComponent);
            }

            foreach (var trainComponent in level.GetComponentsInChildren<Train>())
            {
                trainComponent.name = $"Train {trainComponent.NameOnPanel}";
                EditorUtility.SetDirty(trainComponent);
            }

            foreach (var switchComponent in level.GetComponentsInChildren<SwitchRailwayComponent>())
            {
                switchComponent.name = $"Switch {switchComponent.NameOnPanel}";
                EditorUtility.SetDirty(switchComponent);
            }

            foreach (var exitComponent in level.GetComponentsInChildren<ExitRailwayComponent>())
            {
                exitComponent.name = $"Exit {exitComponent.NameOnPanel}";
                EditorUtility.SetDirty(exitComponent);
            }

            foreach (var namePanel in level.GetComponentsInChildren<NamePanel>())
            {
                namePanel.SendMessage("OnValidate");
                EditorUtility.SetDirty(namePanel);
            }

            var childrenOrdered = level.transform.Cast<Transform>().OrderBy(t => t.name, StringComparer.OrdinalIgnoreCase).ToArray();

            for (var i = 0; i < childrenOrdered.Length; i++)
            {
                childrenOrdered[i].SetSiblingIndex(i);
                EditorUtility.SetDirty(childrenOrdered[i]);
            }

            EditorUtility.SetDirty(level);
            EditorUtility.SetDirty(level.transform);
        }

        private static bool TryAssignBestNextChunkBackwardMatch(RailwayChunk chunk, IReadOnlyCollection<RailwayChunk> chunks)
        {
            foreach (var otherChunk in chunks)
            {
                if (otherChunk == chunk) continue;

                if (otherChunk.WorldDestination == chunk.WorldOrigin)
                {
                    chunk.NextChunkBackward = new RailwayChunkWithDirection(otherChunk, RailwayChunkWithDirection.EDirection.Backward);
                    return true;
                }
            }

            foreach (var otherChunk in chunks)
            {
                if (otherChunk == chunk) continue;

                if (otherChunk.WorldOrigin == chunk.WorldOrigin)
                {
                    chunk.NextChunkBackward = new RailwayChunkWithDirection(otherChunk, RailwayChunkWithDirection.EDirection.Forward);
                    return true;
                }
            }

            return false;
        }

        private static bool TryAssignBestNextChunkForwardMath(RailwayChunk chunk, IReadOnlyCollection<RailwayChunk> chunks)
        {
            foreach (var otherChunk in chunks)
            {
                if (otherChunk == chunk) continue;

                if (otherChunk.WorldOrigin == chunk.WorldDestination)
                {
                    chunk.NextChunkForward = new RailwayChunkWithDirection(otherChunk, RailwayChunkWithDirection.EDirection.Forward);
                    return true;
                }
            }

            foreach (var otherChunk in chunks)
            {
                if (otherChunk == chunk) continue;

                if (otherChunk.WorldDestination == chunk.WorldDestination)
                {
                    chunk.NextChunkForward = new RailwayChunkWithDirection(otherChunk, RailwayChunkWithDirection.EDirection.Backward);
                    return true;
                }
            }

            return false;
        }
    }
}