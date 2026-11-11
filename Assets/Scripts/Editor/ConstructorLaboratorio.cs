// LAD641 · Laboratorio de Iluminación en Unity
// Construye automáticamente el proyecto de la práctica: carpetas, materiales,
// prefab EscenarioBase, las seis escenas configuradas y la Scene List.
// Menú: LAD641 → Construir laboratorio  /  LAD641 → Hornear escenas 04, 05 y 06
// Batchmode: -executeMethod ConstructorLaboratorio.ConstruirYHornearBatch
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class ConstructorLaboratorio
{
    const string Escenas = "Assets/Escenas";
    const string Materiales = "Assets/Materiales";
    const string Prefabs = "Assets/Prefabs";
    const string Texturas = "Assets/Texturas";
    const string RutaPrefab = Prefabs + "/EscenarioBase.prefab";

    static readonly string[] NombresEscenas =
    {
        "01_Directional", "02_Point", "03_Spot", "04_Area", "05_Ambiente_Emisivos", "06_Mixta"
    };

    static readonly Color AmbienteOscuro = Hex("#0B0D18");

    // ───────────────────────────── Menú ─────────────────────────────

    [MenuItem("LAD641/Construir laboratorio", priority = 0)]
    static void ConstruirMenu()
    {
        if (!EditorUtility.DisplayDialog("LAD641 · Laboratorio de Iluminación",
                "Se crearán (o reemplazarán) materiales, el prefab EscenarioBase y las seis escenas en Assets/Escenas.\n\n¿Continuar?",
                "Construir", "Cancelar"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Construir();
        EditorSceneManager.OpenScene($"{Escenas}/{NombresEscenas[0]}.unity");
    }

    [MenuItem("LAD641/Hornear escenas 04, 05 y 06", priority = 1)]
    static void HornearMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        HornearEscenas();
    }

    public static void ConstruirYHornearBatch()
    {
        Construir();
        HornearEscenas();
    }

    // ─────────────────────────── Construcción ───────────────────────────

    public static void Construir()
    {
        foreach (var carpeta in new[] { Escenas, Materiales, Prefabs, Texturas })
            CrearCarpeta(carpeta);

        var mats = CrearMateriales();
        CrearCookie();
        var prefab = CrearPrefab(mats);

        var rutas = new List<string>();
        for (int i = 0; i < NombresEscenas.Length; i++)
        {
            var escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var instancia = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            new GameObject("Selector").AddComponent<SelectorEscenas>();

            var camara = Camera.main;
            camara.transform.SetPositionAndRotation(new Vector3(0f, 4f, -12f), Quaternion.Euler(15f, 0f, 0f));
            var sol = Object.FindFirstObjectByType<Light>();
            sol.name = "Directional Light";

            switch (i)
            {
                case 0: Escena01(sol); break;
                case 1: Escena02(sol, camara, mats); break;
                case 2: Escena03(sol, camara); break;
                case 3: Escena04(sol, camara, instancia); break;
                case 4: Escena05(sol, instancia, mats); break;
                case 5: Escena06(sol, instancia); break;
            }

            string ruta = $"{Escenas}/{NombresEscenas[i]}.unity";
            EditorSceneManager.SaveScene(escena, ruta);
            rutas.Add(ruta);
        }

        var lista = new List<EditorBuildSettingsScene>();
        foreach (var r in rutas) lista.Add(new EditorBuildSettingsScene(r, true));
        EditorBuildSettings.scenes = lista.ToArray();

        AssetDatabase.SaveAssets();
        Debug.Log("[LAD641] Laboratorio construido: 6 escenas en Assets/Escenas y Scene List actualizada.");
    }

    // ─────────────────────────── Escena 01 ───────────────────────────

    static void Escena01(Light sol)
    {
        sol.lightmapBakeType = LightmapBakeType.Realtime;
        sol.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        sol.intensity = 1f;
        sol.shadows = LightShadows.Soft;
        // El Gradient de CicloDiaNoche requiere Light Appearance = Color (ver nota de la guía).
        sol.useColorTemperature = false;
        sol.colorTemperature = 5500f;
        sol.color = Color.white;
        RenderSettings.sun = sol;

        var ciclo = sol.gameObject.AddComponent<CicloDiaNoche>();
        var so = new SerializedObject(ciclo);
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.55f, 0.2f), 0f), new GradientColorKey(new Color(1f, 0.95f, 0.85f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        so.FindProperty("colorDelSol").gradientValue = g;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ─────────────────────────── Escena 02 ───────────────────────────

    static void Escena02(Light sol, Camera camara, SetMateriales mats)
    {
        sol.gameObject.SetActive(false);
        AmbienteColor(AmbienteOscuro);
        FondoNegro(camara);

        var posiciones = new[] { new Vector3(-14f, 2.5f, 3f), new Vector3(-3f, 2.5f, 14f), new Vector3(7f, 2.5f, 14f) };
        var antorchas = new GameObject("Antorchas");
        for (int n = 0; n < posiciones.Length; n++)
        {
            var go = new GameObject($"Antorcha_{n + 1}");
            go.transform.SetParent(antorchas.transform);
            go.transform.position = posiciones[n];
            var luz = go.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.lightmapBakeType = LightmapBakeType.Realtime;
            luz.useColorTemperature = true;
            luz.colorTemperature = 1900f;
            luz.color = Color.white;
            luz.intensity = 3f;
            luz.range = 8f;
            luz.shadows = n == 0 ? LightShadows.Soft : LightShadows.None;   // solo una con sombra
            go.AddComponent<ParpadeoAntorcha>();

            var flama = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flama.name = $"Flama_{n + 1}";
            Object.DestroyImmediate(flama.GetComponent<Collider>());
            flama.transform.position = posiciones[n] + Vector3.down * 0.35f;
            flama.transform.localScale = Vector3.one * 0.2f;
            var r = flama.GetComponent<MeshRenderer>();
            r.sharedMaterial = mats.flama;
            r.shadowCastingMode = ShadowCastingMode.Off;   // que no tape su propia luz
            flama.transform.SetParent(go.transform, true);
        }
    }

    // ─────────────────────────── Escena 03 ───────────────────────────

    static void Escena03(Light sol, Camera camara)
    {
        sol.gameObject.SetActive(false);
        AmbienteColor(AmbienteOscuro);
        FondoNegro(camara);
        camara.transform.SetPositionAndRotation(new Vector3(0f, 1.6f, -8f), Quaternion.Euler(8f, 0f, 0f));
        camara.gameObject.AddComponent<CamaraLibre>();

        var go = new GameObject("Linterna");
        go.transform.SetParent(camara.transform, false);
        go.transform.localPosition = new Vector3(0.3f, -0.2f, 0f);
        go.transform.localRotation = Quaternion.identity;
        var luz = go.AddComponent<Light>();
        luz.type = LightType.Spot;
        luz.lightmapBakeType = LightmapBakeType.Realtime;
        luz.useColorTemperature = true;
        luz.colorTemperature = 6000f;
        luz.color = Color.white;
        luz.intensity = 10f;
        luz.range = 20f;
        luz.innerSpotAngle = 15f;
        luz.spotAngle = 35f;
        luz.shadows = LightShadows.Soft;
        go.AddComponent<Linterna>();
        // Cookie opcional: Assets/Texturas/T_CookieLinterna.png (sin asignar).
    }

    // ─────────────────────────── Escena 04 ───────────────────────────

    static void Escena04(Light sol, Camera camara, GameObject escenario)
    {
        sol.gameObject.SetActive(false);
        AmbienteColor(AmbienteOscuro);
        FondoNegro(camara);
        MarcarEstaticos(escenario, StaticEditorFlags.ContributeGI);

        var go = new GameObject("Area Light");
        go.transform.SetPositionAndRotation(new Vector3(0f, 4f, 0f), Quaternion.Euler(90f, 0f, 0f));
        var luz = go.AddComponent<Light>();
        luz.type = LightType.Rectangle;
        luz.lightmapBakeType = LightmapBakeType.Baked;
        luz.areaSize = new Vector2(4f, 2f);
        luz.intensity = 4f;

        CrearSondas();
        AsignarLightingSettings("LS_04_Area", MixedLightingMode.IndirectOnly);
    }

    // ─────────────────────────── Escena 05 ───────────────────────────

    static void Escena05(Light sol, GameObject escenario, SetMateriales mats)
    {
        sol.gameObject.SetActive(false);
        RenderSettings.ambientMode = AmbientMode.Trilight;            // Source = Gradient
        RenderSettings.ambientSkyColor = new Color(0.22f, 0.32f, 0.52f);
        RenderSettings.ambientEquatorColor = new Color(0.30f, 0.30f, 0.30f);
        RenderSettings.ambientGroundColor = new Color(0.18f, 0.12f, 0.07f);

        var pilar = escenario.transform.Find("Pilar").GetComponent<MeshRenderer>();
        pilar.sharedMaterial = mats.neon;
        PrefabUtility.RecordPrefabInstancePropertyModifications(pilar);

        var placa = GameObject.CreatePrimitive(PrimitiveType.Cube);
        placa.name = "PlacaNeon";
        placa.transform.position = new Vector3(0f, 2.5f, 14.7f);   // pegada a MuroFondo
        placa.transform.localScale = new Vector3(3f, 1f, 0.1f);
        placa.GetComponent<MeshRenderer>().sharedMaterial = mats.neon;

        MarcarEstaticos(escenario, StaticEditorFlags.ContributeGI);
        MarcarEstatico(placa, StaticEditorFlags.ContributeGI);
        AsignarLightingSettings("LS_05_Ambiente_Emisivos", MixedLightingMode.IndirectOnly);
    }

    // ─────────────────────────── Escena 06 ───────────────────────────

    static void Escena06(Light sol, GameObject escenario)
    {
        sol.lightmapBakeType = LightmapBakeType.Mixed;
        sol.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        sol.shadows = LightShadows.Soft;
        RenderSettings.sun = sol;

        var relleno = new[] { new Vector3(-9f, 3f, 9f), new Vector3(9f, 3f, -6f) };
        for (int n = 0; n < relleno.Length; n++)
        {
            var go = new GameObject($"Relleno_Baked_{n + 1}");
            go.transform.position = relleno[n];
            var luz = go.AddComponent<Light>();
            luz.type = LightType.Point;
            luz.lightmapBakeType = LightmapBakeType.Baked;
            luz.useColorTemperature = true;
            luz.colorTemperature = n == 0 ? 3200f : 7500f;
            luz.intensity = 2f;
            luz.range = 12f;
            luz.shadows = LightShadows.Soft;   // sombras horneadas, sin costo en tiempo real
        }

        var movil = escenario.transform.Find("Movil");
        var spotGo = new GameObject("Spot_Realtime");
        spotGo.transform.SetParent(movil, false);
        spotGo.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        spotGo.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);   // gira con Movil y barre el piso
        var spot = spotGo.AddComponent<Light>();
        spot.type = LightType.Spot;
        spot.lightmapBakeType = LightmapBakeType.Realtime;
        spot.color = new Color(1f, 0.35f, 0.35f);
        spot.intensity = 8f;
        spot.range = 12f;
        spot.innerSpotAngle = 20f;
        spot.spotAngle = 40f;
        spot.shadows = LightShadows.Soft;

        MarcarEstaticos(escenario, StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);

        var rpGo = new GameObject("Reflection Probe");
        rpGo.transform.position = new Vector3(0f, 2.5f, 0f);
        var rp = rpGo.AddComponent<ReflectionProbe>();
        rp.mode = ReflectionProbeMode.Baked;
        rp.size = new Vector3(30f, 6f, 30f);
        rp.boxProjection = true;

        CrearSondas();
        AsignarLightingSettings("LS_06_Mixta", MixedLightingMode.Shadowmask);
    }

    // ─────────────────────────── Horneado ───────────────────────────

    public static void HornearEscenas()
    {
        for (int i = 3; i < NombresEscenas.Length; i++)
        {
            string ruta = $"{Escenas}/{NombresEscenas[i]}.unity";
            var escena = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            var inicio = System.DateTime.Now;
            bool ok = Lightmapping.Bake();
            double seg = (System.DateTime.Now - inicio).TotalSeconds;
            EditorSceneManager.SaveScene(escena);
            Debug.Log($"[LAD641] Horneado {NombresEscenas[i]}: {(ok ? "OK" : "FALLÓ")} en {seg:F1} s");
        }
        AssetDatabase.SaveAssets();
    }

    // ─────────────────────────── Materiales ───────────────────────────

    class SetMateriales { public Material piso, muro, rojo, azul, metal, neon, flama; }

    static SetMateriales CrearMateriales()
    {
        var m = new SetMateriales
        {
            piso = Lit("M_Piso", new Color(0.78f, 0.78f, 0.78f), 0.2f),
            muro = Lit("M_Muro", Hex("#EFE9DC"), 0.1f),
            rojo = Lit("M_Rojo", Hex("#CF152D"), 0.7f),
            azul = Lit("M_Azul", Hex("#0E1D61"), 0.4f),
            metal = Lit("M_Metal", new Color(0.6f, 0.6f, 0.6f), 0.9f, 1f),
            neon = Lit("M_Neon", Color.black, 0.5f),
            flama = Lit("M_Flama", Color.black, 0.5f),
        };
        // Intensidad HDR 3 en el selector de color de Unity = color × 2^3
        Emision(m.neon, new Color(0f, 1f, 1f) * 8f, MaterialGlobalIlluminationFlags.BakedEmissive);
        Emision(m.flama, new Color(1f, 0.45f, 0.05f) * 4f, MaterialGlobalIlluminationFlags.None);
        AssetDatabase.SaveAssets();
        return m;
    }

    static Material Lit(string nombre, Color color, float smoothness, float metallic = 0f)
    {
        string ruta = $"{Materiales}/{nombre}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, ruta);
        }
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void Emision(Material mat, Color hdr, MaterialGlobalIlluminationFlags gi)
    {
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", hdr);
        mat.globalIlluminationFlags = gi;
        EditorUtility.SetDirty(mat);
    }

    // ─────────────────────────── Prefab ───────────────────────────

    static GameObject CrearPrefab(SetMateriales m)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var raiz = new GameObject("EscenarioBase");

        Primitiva(raiz, PrimitiveType.Plane, "Piso", Vector3.zero, new Vector3(3f, 1f, 3f), m.piso);
        Primitiva(raiz, PrimitiveType.Cube, "MuroFondo", new Vector3(0f, 2.5f, 15f), new Vector3(30f, 5f, 0.5f), m.muro);
        Primitiva(raiz, PrimitiveType.Cube, "MuroIzq", new Vector3(-15f, 2.5f, 0f), new Vector3(0.5f, 5f, 30f), m.muro);
        Primitiva(raiz, PrimitiveType.Sphere, "Esfera", new Vector3(-2f, 1f, 0f), Vector3.one, m.rojo);
        Primitiva(raiz, PrimitiveType.Cube, "Caja", new Vector3(2f, 1f, 1f), Vector3.one, m.azul);
        Primitiva(raiz, PrimitiveType.Cylinder, "Pilar", new Vector3(0f, 2f, 4f), new Vector3(1f, 2f, 1f), m.muro);
        Primitiva(raiz, PrimitiveType.Sphere, "EsferaMetal", new Vector3(4f, 1f, -2f), Vector3.one, m.metal);
        var movil = Primitiva(raiz, PrimitiveType.Capsule, "Movil", new Vector3(-4f, 1f, -2f), Vector3.one, m.rojo);
        movil.AddComponent<Rotador>();

        var prefab = PrefabUtility.SaveAsPrefabAsset(raiz, RutaPrefab);
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    static GameObject Primitiva(GameObject padre, PrimitiveType tipo, string nombre, Vector3 pos, Vector3 escala, Material mat)
    {
        var go = GameObject.CreatePrimitive(tipo);
        go.name = nombre;
        go.transform.SetParent(padre.transform, false);
        go.transform.localPosition = pos;
        go.transform.localScale = escala;
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    // ─────────────────────────── Utilidades ───────────────────────────

    static void MarcarEstaticos(GameObject escenario, StaticEditorFlags flags)
    {
        foreach (Transform hijo in escenario.transform)
            if (hijo.name != "Movil")
                MarcarEstatico(hijo.gameObject, flags);
    }

    static void MarcarEstatico(GameObject go, StaticEditorFlags flags)
    {
        GameObjectUtility.SetStaticEditorFlags(go, flags);
        var r = go.GetComponent<MeshRenderer>();
        r.receiveGI = ReceiveGI.Lightmaps;
        if (PrefabUtility.IsPartOfPrefabInstance(go))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(go);
            PrefabUtility.RecordPrefabInstancePropertyModifications(r);
        }
    }

    static void CrearSondas()
    {
        var go = new GameObject("Light Probe Group");
        var grupo = go.AddComponent<LightProbeGroup>();
        var pos = new List<Vector3>();
        for (float x = -12f; x <= 12f; x += 3f)
            for (float z = -12f; z <= 12f; z += 3f)
                foreach (float y in new[] { 0.5f, 2.5f, 4.5f })
                    pos.Add(new Vector3(x, y, z));
        // Más densas bajo el panel de luz (centro del escenario)
        for (float x = -4f; x <= 4f; x += 1f)
            for (float z = -3f; z <= 3f; z += 1f)
                foreach (float y in new[] { 1f, 2f })
                    pos.Add(new Vector3(x + 0.5f, y, z + 0.5f));
        grupo.probePositions = pos.ToArray();
    }

    static void AsignarLightingSettings(string nombre, MixedLightingMode modo)
    {
        string ruta = $"{Escenas}/{nombre}.lighting";
        AssetDatabase.DeleteAsset(ruta);
        var ls = new LightingSettings
        {
            name = nombre,
            bakedGI = true,
            realtimeGI = false,
            lightmapper = LightingSettings.Lightmapper.ProgressiveGPU,
            mixedBakeMode = modo,
            lightmapResolution = 20f,   // baja para hornear rápido en el aula
            lightmapPadding = 2,
            lightmapMaxSize = 1024,
            directSampleCount = 32,
            indirectSampleCount = 256,
            environmentSampleCount = 256,
        };
        AssetDatabase.CreateAsset(ls, ruta);
        Lightmapping.lightingSettings = ls;
    }

    static void AmbienteColor(Color c)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;   // Source = Color
        RenderSettings.ambientLight = c;
        RenderSettings.reflectionIntensity = 0.2f;
    }

    static void FondoNegro(Camera cam)
    {
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
    }

    static void CrearCookie()
    {
        string ruta = $"{Texturas}/T_CookieLinterna.png";
        if (File.Exists(ruta)) return;
        const int n = 256;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = (x - n / 2f) / (n / 2f), dy = (y - n / 2f) / (n / 2f);
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float circulo = 1f - Mathf.SmoothStep(0.75f, 0.95f, d);
                float anillo = 1f - 0.35f * Mathf.Exp(-Mathf.Pow((d - 0.45f) / 0.06f, 2f));
                float ruido = 0.8f + 0.2f * Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float v = Mathf.Clamp01(circulo * anillo * ruido);
                tex.SetPixel(x, y, new Color(v, v, v, v));
            }
        File.WriteAllBytes(ruta, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(ruta);
        var imp = (TextureImporter)AssetImporter.GetAtPath(ruta);
        imp.textureType = TextureImporterType.Cookie;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.alphaSource = TextureImporterAlphaSource.FromGrayScale;
        imp.SaveAndReimport();
    }

    static void CrearCarpeta(string ruta)
    {
        if (AssetDatabase.IsValidFolder(ruta)) return;
        AssetDatabase.CreateFolder("Assets", ruta.Substring("Assets/".Length));
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
