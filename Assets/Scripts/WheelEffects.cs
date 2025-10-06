using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        public Transform SkidTrailPrefab;
        public static Transform skidTrailsDetachedParent;
        //public ParticleSystem skidParticles;
        public bool skidding { get; private set; }
        public bool PlayingAudio { get; private set; }


        private AudioSource m_AudioSource;
        private Transform[] m_SkidTrail = new Transform[3];
        private WheelCollider m_WheelCollider;


        private void Start()
        {
            //skidParticles = transform.root.GetComponentInChildren<ParticleSystem>();

            //if (skidParticles == null)
            //{
            //    Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
            //}
            //else
            //{
            //    skidParticles.Stop();
            //}

            m_WheelCollider = GetComponent<WheelCollider>();
            m_AudioSource = GetComponent<AudioSource>();
            PlayingAudio = false;

            if (skidTrailsDetachedParent == null)
            {
                skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }


        //public void EmitTyreSmoke()
        //{
        //    skidParticles.transform.position = transform.position - transform.up*m_WheelCollider.radius;
        //    skidParticles.Emit(1);
        //    if (!skidding)
        //    {
        //        StartCoroutine(StartSkidTrail());
        //    }
        //}


        public void PlayAudio()
        {
            m_AudioSource.Play();
            PlayingAudio = true;
        }


        public void StopAudio()
        {
            m_AudioSource.Stop();
            PlayingAudio = false;
        }


        public IEnumerator StartSkidTrail()
        {
            skidding = true;
            for (int i = 0; i < m_SkidTrail.Length; i++)
            {
                m_SkidTrail[i] = Instantiate(SkidTrailPrefab);
            }
            
            while (m_SkidTrail == null)
            {
                yield return null;
            }
            for (int i = 0; i < m_SkidTrail.Length; i++)
            {
                m_SkidTrail[i].parent = transform;
                m_SkidTrail[i].localPosition = -Vector3.up * m_WheelCollider.radius;
            }
            
        }


        public void EndSkidTrail()
        {
            if (!skidding)
            {
                return;
            }
            skidding = false;
            for (int i = 0; i < m_SkidTrail.Length; i++)
            {
                m_SkidTrail[i].parent = skidTrailsDetachedParent;
                Destroy(m_SkidTrail[i].gameObject, 5.0f);
            }
        }
    }
}
