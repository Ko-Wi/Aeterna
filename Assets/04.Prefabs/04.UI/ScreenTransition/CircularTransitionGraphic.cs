using UnityEngine;

// 기존 참조를 유지하면서 스프라이트 모양의 역마스크를 그립니다.
[ExecuteAlways]
public sealed class CircularTransitionGraphic : UnityEngine.UI.MaskableGraphic
{
    [SerializeField] private Sprite maskSprite;
    [SerializeField] private Shader transitionShader;
    [SerializeField, Range(0f,1f)] private float openness = 1f;
    private Material ownedMaterial;
    protected override void OnEnable() { base.OnEnable(); EnsureMaterial(); }
    private void EnsureMaterial()
    {
        if (ownedMaterial == null && transitionShader != null) {
            ownedMaterial = new Material(transitionShader) { hideFlags = HideFlags.HideAndDontSave };
            material = ownedMaterial;
        }
    }
    public override Texture mainTexture => maskSprite != null ? maskSprite.texture : Texture2D.whiteTexture;
    protected override void UpdateMaterial()
    {
        EnsureMaterial();
        if (ownedMaterial != null && maskSprite != null)
            ownedMaterial.SetVector("_SpriteUV", UnityEngine.Sprites.DataUtility.GetOuterUV(maskSprite));
        base.UpdateMaterial();
    }
    protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
    {
        vh.Clear(); Rect r = GetPixelAdjustedRect();
        if (r.width <= 0 || r.height <= 0) return;
        float aspect = maskSprite == null ? 1 : maskSprite.rect.width / maskSprite.rect.height;
        for (int i=0;i<4;i++) {
            Vector2 uv = new Vector2(i==1 || i==2 ? 1:0, i>=2 ? 1:0);
            var v = UnityEngine.UIVertex.simpleVert;
            v.position = new Vector3(r.xMin+uv.x*r.width,r.yMin+uv.y*r.height);
            v.color=color; v.uv0=uv;
            v.uv1=new Vector4(openness,r.width/r.height/aspect,0,0);
            vh.AddVert(v);
        }
        vh.AddTriangle(0,1,2); vh.AddTriangle(2,3,0);
    }
    protected override void OnDidApplyAnimationProperties() { base.OnDidApplyAnimationProperties(); SetVerticesDirty(); }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (ownedMaterial != null) { if(Application.isPlaying) Destroy(ownedMaterial); else DestroyImmediate(ownedMaterial); }
    }
}
