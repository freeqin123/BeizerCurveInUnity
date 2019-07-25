using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SplineMesh
{
    /// <summary>
    /// 存放物体在运动到最后一个状态时，是循环移动还是停止
    /// </summary>
    public enum MoveMode
    {
        loopInFinish,
        stopInFinish
    }
    public class CameraTrailManager : MonoBehaviour
    {
        public LayerMask layMsk;
        public float normalOffset = 0.1f;
        public static CameraTrailManager instance;
        public bool activeCreateNode = false;

        #region 沿路径移动物体功能参数
        [Header("需要移动的物体")]
        public GameObject follower;
        [Header("控制物体移动速度，值越大移动速度越慢，值越小移动越快，负数为反向移动")]
        public float DurationInSecond = 5f;
        [Header("设置动画结束状态")]
        public MoveMode playMode = MoveMode.stopInFinish;
        [HideInInspector]
        public float rate = 0;
        [HideInInspector]
        public bool isStartMove = false;
        #endregion

        private Spline m_Spline;
        public List<GameObject> handlerGoup = new List<GameObject>();
        public bool isShowPath = false;

        private GameObject currentSelectObj;
        private int needRemoveIndex;
        [HideInInspector]
        public bool isFirstCreate = true;
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                return;
            }

            m_Spline = transform.GetComponent<Spline>();
            InitNodeGroup();

            EndEditPath();
        }
        //初始化，生成控制点
        public void InitNodeGroup()
        {
            //清空临时存储list
            if (handlerGoup.Count != 0)
            {
                foreach (var item in handlerGoup)
                {
                    Destroy(item.gameObject);
                }
                handlerGoup.Clear();
            }

            for (int i = 0; i < m_Spline.nodes.Count; i++)
            {
                GameObject movePoint = Instantiate(Resources.Load<GameObject>("MoveHandlerPoint"));
                handlerGoup.Add(movePoint);
                movePoint.transform.position = m_Spline.nodes[i].Position;
                movePoint.name = "MovePoint/" + i;
            }

            if (isFirstCreate)
            {
                HidePathRender();
                return;
            }
        }

        //遍历数组，使控制点和spline中的node的坐标位置相同
        public void ConnectMovePointToNode()
        {
            if (handlerGoup.Count == m_Spline.nodes.Count)
            {
                for (int i = 0; i < handlerGoup.Count; i++)
                {
                    m_Spline.nodes[i].Position = handlerGoup[i].transform.position;
                }
            }
        }

        private void Update()
        {
            if (activeCreateNode)
            {
                ConnectMovePointToNode();

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitPoint;
                if (Physics.Raycast(ray, out hitPoint, layMsk))
                {
                    //print(hitPoint.point);
                    //print("法线" + hitPoint.normal);
                    if ((Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButtonDown(0)) || (Input.GetKey(KeyCode.RightControl) && Input.GetMouseButtonDown(0)))
                    {
                        Vector3 pos = new Vector3();
                        pos = hitPoint.point + hitPoint.normal * normalOffset;
                        if (isFirstCreate)
                        {
                            SpecialCreateNode(pos);
                        }
                        else
                        {
                            print("生成新节点");
                            CreateNewNode(pos);
                        }
                    }
                }


                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    print("删除节点");
                    DeleteNote();
                }

                if (isStartMove)
                {
                    ObjMoveByPath();
                }
                else
                {
                    return;
                }
            }
        }
        /// <summary>
        /// 根据鼠标点击生成路径控制点
        /// </summary>
        private void CreateNewNode(Vector3 nodePostion)
        {
            SplineNode newNode = new SplineNode(nodePostion, m_Spline.nodes[m_Spline.nodes.Count - 1].Direction);
            m_Spline.AddNode(newNode);
            InitNodeGroup();
            StartCoroutine(ClosePathShadow(0.2f));
        }
        //特殊方式来创建节点
        private void SpecialCreateNode(Vector3 nodePostion)
        {
            m_Spline.nodes[m_Spline.nodes.Count - 1].Position = nodePostion;
            ShowPathRender();
            isFirstCreate = false;
            InitNodeGroup();
            StartCoroutine(ClosePathShadow(0.2f));
        }

        /// <summary>
        /// 按Delet键删除生成的节点
        /// </summary>
        private void DeleteNote()
        {
            if (m_Spline.nodes.Count > 2)
            {
                for (int i = 0; i < handlerGoup.Count; i++)
                {
                    if (handlerGoup[i].GetComponent<ItemPointer>().isSelect == true && handlerGoup.Count == m_Spline.nodes.Count)
                    {
                        currentSelectObj = handlerGoup[i];
                        needRemoveIndex = i;
                        Destroy(handlerGoup[i].gameObject);
                        handlerGoup.Remove(handlerGoup[i]);
                        m_Spline.RemoveNode(m_Spline.nodes[i]);
                    }
                }
                InitNodeGroup();
            }
            else
            {
                isFirstCreate = true;
                HidePathRender();
            }
        }
        /// <summary>
        /// 使物体沿着绘制的路径点进行移动
        /// </summary>
        private void ObjMoveByPath()
        {
            rate += Time.deltaTime / DurationInSecond;

            switch (playMode)
            {
                case MoveMode.loopInFinish:
                    LoopModeMove();
                    break;
                case MoveMode.stopInFinish:
                    StopModeMove();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Loop模式下进行移动
        /// </summary>
        private void LoopModeMove()
        {
            if (follower != null)
            {
                if (DurationInSecond != 0)
                {
                    rate += Time.deltaTime / DurationInSecond;
                    if (rate > m_Spline.nodes.Count - 1)
                    {
                        rate -= m_Spline.nodes.Count - 1;
                    }
                    //使物体按照路径移动
                    CurveSample sample = m_Spline.GetSample(rate);
                    follower.transform.position = sample.location;
                }
            }
            else
            {
                Debug.LogWarning("Follew参数为空!");
                return;
            }
        }
        /// <summary>
        /// Stop模式下进行移动
        /// </summary>
        private void StopModeMove()
        {
            if (follower != null)
            {
                if (DurationInSecond != 0)
                {
                    rate += Time.deltaTime / DurationInSecond;
                }
                if (rate > m_Spline.nodes.Count - 1)
                {
                    rate = m_Spline.nodes.Count - 1;
                    StopMoveOnPath();
                }
                //使物体按照路径移动
                CurveSample sample = m_Spline.GetSample(rate);
                follower.transform.position = sample.location;
            }
            else
            {
                Debug.LogWarning("Follew参数为空!");
                return;
            }
        }
        /// <summary>
        /// 使沿路径的移动停止
        /// </summary>
        public void StopMoveOnPath()
        {
            isStartMove = false;
        }
        /// <summary>
        /// 停止后继续播放，该方法配合StopMoveOnPath一起使用
        /// </summary>
        public void GoOnPlay()
        {
            isStartMove = true;
        }
        /// <summary>
        /// 开始沿路径上的移动
        /// </summary>
        public void RestartMoveOnPath()
        {
            rate = 0f;
        }
        /// <summary>
        /// 隐藏路径显示
        /// </summary>
        public void HidePathRender()
        {
            MeshRenderer[] renderGroup;
            renderGroup = transform.GetComponentsInChildren<MeshRenderer>();

            foreach (var item in renderGroup)
            {
                item.enabled = false;
            }

            foreach (var item in handlerGoup)
            {
                item.GetComponent<MeshRenderer>().enabled = false;
            }
            isShowPath = false;
        }
        /// <summary>
        /// 显示路径
        /// </summary>
        public void ShowPathRender()
        {
            MeshRenderer[] renderGroup;
            renderGroup = transform.GetComponentsInChildren<MeshRenderer>();

            foreach (var item in renderGroup)
            {
                item.enabled = true;
            }

            foreach (var item in handlerGoup)
            {
                item.GetComponent<MeshRenderer>().enabled = true;
            }
            isShowPath = true;
        }
        /// <summary>
        /// 设置起始Node的坐标
        /// </summary>
        public void SetFistNodePos(Vector3 pos)
        {
            m_Spline.nodes[0].Position = pos;
        }
        /// <summary>
        /// 关掉路径的阴影
        /// </summary>
        IEnumerator ClosePathShadow(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            //把路径的阴影关掉
            MeshRenderer[] splineMeshItem = transform.GetComponentsInChildren<MeshRenderer>();
            foreach (var item in splineMeshItem)
            {
                if (item.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On)
                    item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                if (item.receiveShadows == true)
                    item.receiveShadows = false;
                //将其层级设为12 为了不让指定相机显示
                item.gameObject.layer = 12;
            }
            StopAllCoroutines();
        }
        /// <summary>
        /// 开始编辑路径点
        /// </summary>
        public void BeginEditPath()
        {
            gameObject.SetActive(true);
            activeCreateNode = true;
        }
        /// <summary>
        /// 结束编辑路径点
        /// </summary>
        public void EndEditPath()
        {
            gameObject.SetActive(false);
            activeCreateNode = false;
            HidePathRender();
        }
    }
}
