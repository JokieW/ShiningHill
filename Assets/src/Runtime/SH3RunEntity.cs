using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SH3RunEntity : MonoBehaviour
{
    [SerializeField] public int id;
    [SerializeField][HideInInspector] private int _selfBaseAddress;
    [SerializeField][HideInInspector] public StateChecker checker;

    public float health;
    public float maxHealth;

    //General
    private const int _globalBaseAddress = 0x008984E0;
    private const int _size = 0x02F0;

    //Transform
    [SerializeField] private SHPtr v3_position;
    [SerializeField] private SHPtr v3_rotation;
    [SerializeField] private SHPtr v3_scale;
    [SerializeField] private SHPtr v3_forwardVector;
    [SerializeField] private SHPtr v3_rightVector;
    [SerializeField] private SHPtr v3_upVector;

    //Flags
    [SerializeField] private SHPtr bits_unknownFlags1;
    [SerializeField] private SHPtr bits_unknownFlags2;
    [SerializeField] private SHPtr bits_unknownFlags3;
    [SerializeField] private SHPtr bits_unknownFlags4;

    //Enums
    [SerializeField] private SHPtr int_movementEnum;

    //Status
    [SerializeField] private SHPtr float_health;
    [SerializeField] private SHPtr float_maxHealth;
    [SerializeField] private SHPtr float_healthPercent;

    void Start()
    {
        _selfBaseAddress = _globalBaseAddress + (_size * id);

        v3_position = _selfBaseAddress + 0x0000;
        v3_rotation = _selfBaseAddress + 0x0010;
        v3_scale = _selfBaseAddress + 0x0088;
        v3_forwardVector = _selfBaseAddress + 0x0040;
        v3_rightVector = _selfBaseAddress + 0x0020;
        v3_upVector = _selfBaseAddress + 0x0030;

        bits_unknownFlags1 = _selfBaseAddress + 0x0078;
        bits_unknownFlags2 = _selfBaseAddress + 0x0079;
        bits_unknownFlags3 = _selfBaseAddress + 0x007A;
        bits_unknownFlags4 = _selfBaseAddress + 0x007B;

        int_movementEnum = _selfBaseAddress + 0x007C;

        float_health = _selfBaseAddress + 0x0180;
        float_maxHealth = _selfBaseAddress + 0x0184;
        float_healthPercent = _selfBaseAddress + 0x0188;

        UnityEngine.Object prefab = Resources.Load("Prefabs/EntityVisual");
        Instantiate(prefab, transform);
    }

    void Update ()
    {
        IntPtr handle = checker.memHandle;
        if (handle != IntPtr.Zero)
        {
            transform.position = (Scribe.ReadVector3(handle, v3_position));
            transform.rotation = Quaternion.Euler(Scribe.ReadVector3(handle, v3_rotation) * Mathf.Rad2Deg);
            transform.localScale = Scribe.ReadVector3(handle, v3_scale);

            health = Scribe.ReadSingle(handle, float_health);
            maxHealth = Scribe.ReadSingle(handle, float_maxHealth);
        }
    }

    private static readonly Dictionary<int, string> _knownEntities = new Dictionary<int, string>()
    {
        {0, "Heather"},
        {1, "Heather Reflection"},
    };

    public static void CreateEntities(StateChecker checker)
    {
        checker.livingEntities = new GameObject[32];

        for (int i = 0; i != checker.livingEntities.Length; i++)
        {
            string name;
            if (!_knownEntities.TryGetValue(i, out name)) name = "Entity " + i;
            GameObject go = new GameObject(name);

            SH3RunEntity ent = go.AddComponent<SH3RunEntity>();
            ent.id = i;
            ent.checker = checker;

            checker.livingEntities[i] = go;
            ent.transform.parent = checker.Entities.transform;
        }
    }
}
