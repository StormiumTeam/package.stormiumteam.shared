using UnityEngine;
using UnityEngine.UI;

namespace package.stormiumteam.shared.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AdjustGridLayoutCellSize : MonoBehaviour
    {
        public enum Axis
        {
            X,
            Y
        };

        public enum RatioMode
        {
            Free,
            Fixed
        };
    
        [SerializeField] private Axis      expand;
        [SerializeField] private RatioMode ratioMode;
        [SerializeField] private float     cellRatio = 1;

        private new RectTransform   transform;
        private     GridLayoutGroup grid;

        private void Awake()
        {
            transform = (RectTransform) base.transform;
            grid      = GetComponent<GridLayoutGroup>();
        }

        // Start is called before the first frame update
        private void Start()
        {
            UpdateCellSize();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
        }

#if UNITY_EDITOR
        [ExecuteAlways]
        private void Update()
        {
            UpdateCellSize();
        }
#endif

        private void OnValidate()
        {
            transform = (RectTransform) base.transform;
            grid      = GetComponent<GridLayoutGroup>();
            UpdateCellSize();
        }

        private void UpdateCellSize()
        {
            var count = grid.constraintCount;
            if (expand == Axis.X)
            {
                var spacing     = (count - 1) * grid.spacing.x;
                var contentSize = transform.rect.width - grid.padding.left - grid.padding.right - spacing;
                var sizePerCell = contentSize / count;
                grid.cellSize = new Vector2(sizePerCell, ratioMode == RatioMode.Free ? grid.cellSize.y : sizePerCell * cellRatio);

            }
            else //if (expand == Axis.Y)
            {
                var spacing     = (count - 1) * grid.spacing.y;
                var contentSize = transform.rect.height - grid.padding.top - grid.padding.bottom - spacing;
                var sizePerCell = contentSize / count;
                grid.cellSize = new Vector2(ratioMode == RatioMode.Free ? grid.cellSize.x : sizePerCell * cellRatio, sizePerCell);
            }
        }
    }
}