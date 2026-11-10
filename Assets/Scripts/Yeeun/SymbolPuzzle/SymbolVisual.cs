using UnityEngine;

namespace Yeeun.SymbolPuzzle
{
    /// <summary>
    /// Shared base for objects that display a symbol (wall relief, floor plate).
    /// Changing <see cref="Shape"/> in the Inspector swaps the mesh from the library.
    /// Colours are applied with a MaterialPropertyBlock so shared materials are never modified.
    /// </summary>
    public abstract class SymbolVisual : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] SymbolShape shape = SymbolShape.Circle;
        [SerializeField] SymbolMeshLibrary meshLibrary;
        [Tooltip("Renderer that shows the symbol mesh (usually a child object).")]
        [SerializeField] protected MeshRenderer symbolRenderer;

        MaterialPropertyBlock block;

        public SymbolShape Shape
        {
            get => shape;
            set
            {
                shape = value;
                ApplyShape();
            }
        }

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            // Changing other components inside OnValidate is unsafe, so defer one editor tick.
            UnityEditor.EditorApplication.delayCall += () => { if (this != null) ApplyShape(); };
#endif
        }

        protected virtual void Awake() => ApplyShape();

        /// <summary>Puts the mesh for <see cref="Shape"/> on the symbol renderer.</summary>
        public void ApplyShape()
        {
            if (symbolRenderer == null || meshLibrary == null) return;
            var mesh = meshLibrary.Get(shape);
            var filter = symbolRenderer.GetComponent<MeshFilter>();
            if (mesh == null || filter == null || filter.sharedMesh == mesh) return;
            filter.sharedMesh = mesh;
#if UNITY_EDITOR
            // Keep the swap as a prefab override so it is saved with the scene/prefab.
            if (!Application.isPlaying && UnityEditor.PrefabUtility.IsPartOfPrefabInstance(filter))
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
#endif
        }

        /// <summary>Tints the symbol. Pass null to restore the material's own colour.</summary>
        protected void SetTint(Color? color)
        {
            if (symbolRenderer == null) return;
            if (color == null)
            {
                symbolRenderer.SetPropertyBlock(null);
                return;
            }
            block ??= new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color.Value);
            block.SetColor(ColorId, color.Value);
            symbolRenderer.SetPropertyBlock(block);
        }
    }
}
