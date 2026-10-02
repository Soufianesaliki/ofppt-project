using UnityEngine;

namespace ElectricalWorkshop.Components
{
    /// <summary>
    /// Swaps the base image shown on a whiteboard plane. Attach to the whiteboard's parent
    /// object and assign the child plane's Renderer to _targetRenderer.
    ///
    /// Material handling follows VoyantController / PushButtonController: a per-renderer
    /// MaterialPropertyBlock, never the shared material. Both whiteboards can therefore use
    /// the same material asset and each still changes only its own plane.
    ///
    /// Nothing is applied on Awake/Start: the material's own default image stays visible until
    /// the first ShowImage()/ShowEnd() call. Driven externally (by ScenarioFlowManager).
    ///
    /// Texture requirements: URP Lit/Simple Lit/Unlit use "_BaseMap"; Built-in Standard uses
    /// "_MainTex". Tiling/offset come from the material and are not touched here.
    /// </summary>
    [DisallowMultipleComponent]
    public class WhiteboardImageController : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The whiteboard plane's Renderer (a child of this object).")]
        [SerializeField] private Renderer _targetRenderer;

        [Tooltip("Index into _targetRenderer.sharedMaterials to change. Use 0 if it only has one material.")]
        [SerializeField] private int _materialIndex = 0;

        [Tooltip("URP Lit / Simple Lit / Unlit: \"_BaseMap\". Built-in Standard: \"_MainTex\".")]
        [SerializeField] private string _texturePropertyName = "_BaseMap";

        [Header("Images")]
        [Tooltip("Instruction images, addressed by index through ShowImage(int).")]
        [SerializeField] private Texture2D[] _images;

        [Tooltip("Shown by ShowEnd() when the scenario concludes.")]
        [SerializeField] private Texture2D _endImage;

        private MaterialPropertyBlock _mpb;
        private int _texturePropertyId;

        /// <summary>Number of instruction images assigned (excludes the end image).</summary>
        public int imageCount => _images?.Length ?? 0;

        private void Awake()
        {
            _texturePropertyId = Shader.PropertyToID(_texturePropertyName);
            _mpb = new MaterialPropertyBlock();
        }

        /// <summary>Shows instruction image <paramref name="index"/> from the list.</summary>
        public void ShowImage(int index)
        {
            if (_images == null || index < 0 || index >= _images.Length)
            {
                Debug.LogWarning($"WhiteboardImageController '{name}': no image at index {index} (list has {imageCount}).", this);
                return;
            }

            Apply(_images[index]);
        }

        /// <summary>Shows the end-of-scenario image.</summary>
        public void ShowEnd()
        {
            if (_endImage == null)
            {
                Debug.LogWarning($"WhiteboardImageController '{name}': no end image assigned.", this);
                return;
            }

            Apply(_endImage);
        }

        private void Apply(Texture texture)
        {
            if (_targetRenderer == null)
            {
                Debug.LogWarning($"WhiteboardImageController '{name}': no target renderer assigned.", this);
                return;
            }

            if (texture == null)
            {
                Debug.LogWarning($"WhiteboardImageController '{name}': the requested image slot is empty.", this);
                return;
            }

            // Awake may not have run if called from an inactive object's earlier lifecycle.
            _mpb ??= new MaterialPropertyBlock();
            if (_texturePropertyId == 0) _texturePropertyId = Shader.PropertyToID(_texturePropertyName);

            int index = Mathf.Clamp(_materialIndex, 0, _targetRenderer.sharedMaterials.Length - 1);

            _targetRenderer.GetPropertyBlock(_mpb, index);
            _mpb.SetTexture(_texturePropertyId, texture);
            _targetRenderer.SetPropertyBlock(_mpb, index);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _texturePropertyId = Shader.PropertyToID(_texturePropertyName);
            if (_targetRenderer != null)
                _materialIndex = Mathf.Clamp(_materialIndex, 0, Mathf.Max(0, _targetRenderer.sharedMaterials.Length - 1));
        }
#endif
    }
}
