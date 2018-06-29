using System.Collections.Generic;
using UnityEngine;

public class PhysicalParticleSimController : MonoBehaviour, ISerializationCallbackReceiver
{
    public GameObject ReferenceParticleObject;

    [Range(0, 500)]
    [SerializeField]
    private int _NumberOfParticles = 10;

    [Range(0.1f, 10f)]
    [SerializeField]
    private float _ParticleEnergy = 1f;

    [Range(0f, 10f)]
    [SerializeField]
    private int _NumberOfParticleTrails = 5;

    public int NumberOfParticleTrails
    {
        get { return _NumberOfParticleTrails; }
        set { _NumberOfParticleTrails = value; }
    }

    public int NumberOfParticles
    {
        get
        {
            return _NumberOfParticles;
        }
        set
        {
            _NumberOfParticles = value;
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                return;
#endif
            SetNumberOfRezzedParticles(value);
        }
    }

    public float NumberOfParticlesF
    {
        get
        {
            return NumberOfParticles;
        }
        set
        {
            NumberOfParticles = Mathf.FloorToInt(value);
        }
    }

    public float ParticleEnergy
    {
        get { return _ParticleEnergy; }
        set { _ParticleEnergy = value; }
    }

    private List<Rigidbody> RezzedParticleObjects = new List<Rigidbody>();

    private Rigidbody RezParticleObject(GameObject reference_particle_object)
    {
        var particle_object = Instantiate(reference_particle_object);
        particle_object.transform.SetParent(transform, false);
        var position = 0.5f * Random.insideUnitSphere;
        position.Scale(transform.lossyScale);
        particle_object.transform.localPosition = position;

        var particle_rb = particle_object.GetComponent<Rigidbody>();
        particle_rb.velocity = ParticleEnergy * Random.insideUnitSphere;

        return particle_rb;
    }

    //private static void RemoveAndDerez(List<GameObject> objects, int index)
    //{
    //    var @object = objects[index];
    //    objects.RemoveAt(index);
    //    Destroy(@object);
    //}

    private static void RemoveAndDerez<TObject>(List<TObject> objects, int index) where TObject : Component
    {
        var @object = objects[index];
        objects.RemoveAt(index);
        Destroy(@object.gameObject);
    }

    //private static void PopAndDerez(List<GameObject> objects)
    //{
    //    RemoveAndDerez(objects, objects.Count - 1);
    //}

    private static void PopAndDerez<TObject>(List<TObject> objects) where TObject : Component
    {
        RemoveAndDerez(objects, objects.Count - 1);
    }

    public void SetNumberOfRezzedParticles(int number_of_particles)
    {
        if (number_of_particles > RezzedParticleObjects.Count)
        {
            for (int i = RezzedParticleObjects.Count; i < number_of_particles; i++)
            {
                // Rez + Push
                var particle_object = RezParticleObject(ReferenceParticleObject);
                RezzedParticleObjects.Add(particle_object);
                if (i < NumberOfParticleTrails)
                    particle_object.GetComponent<TrailRenderer>().enabled = true;
            }
        }
        else if (number_of_particles < RezzedParticleObjects.Count)
        {
            for (int i = RezzedParticleObjects.Count; i > number_of_particles; i--)
                PopAndDerez(RezzedParticleObjects); // Pop + Derez
        }
    }

    private void EnforceParticleEnergyLevels()
    {
        for (int i = 0; i < RezzedParticleObjects.Count; i++)
        {
            var particle = RezzedParticleObjects[i];
            particle.velocity = ParticleEnergy * particle.velocity.normalized;
        }
    }

    private void Start()
    {
        // Round-trip
        NumberOfParticles = NumberOfParticles;
    }

    private void Update()
    {
        EnforceParticleEnergyLevels();
    }

    public void OnBeforeSerialize()
    {
        // Round-trip
        NumberOfParticles = NumberOfParticles;
    }

    public void OnAfterDeserialize()
    {
    }
}