import * as THREE from 'three';
import { FBXLoader } from 'three/addons/loaders/FBXLoader.js';
import {
  createNintendoGrassTexture,
  createResortWaterTexture,
  createStylizedCloud,
  createGlowSpriteTexture,
  addGrandAceDomeShell,
  createFloatingBalloonsGroup,
  createFestiveFireworksSystem
} from './procedural-assets.js';
import { ModelInspectModalViewer } from './model-inspector.js';

/* =========================================================================
   TennisIslandCinematicViewer: 메가 테니스 아일랜드 & 3대 경기장 시네마틱 뷰어
========================================================================= */
class TennisIslandCinematicViewer {
  constructor(canvasContainerId) {
    this.container = document.getElementById(canvasContainerId);
    if (!this.container) return;

    this.clock = new THREE.Clock();
    this.textureLoader = new THREE.TextureLoader();

    this.scene = null;
    this.camera = null;
    this.renderer = null;

    // Groups
    this.islandGroup = new THREE.Group();
    this.leftStadiumGroup = new THREE.Group();
    this.topRightStadiumGroup = new THREE.Group();
    this.bottomRightStadiumGroup = new THREE.Group();

    // 3개 경기장 각각의 중심 좌표 (테니스, 볼링, 검술)
    this.tennisStadiumCenter = new THREE.Vector3(-160, 1.2, 0);
    this.bowlingStadiumCenter = new THREE.Vector3(130, 1.2, -150);
    this.swordStadiumCenter = new THREE.Vector3(130, 1.2, 150);
    this.leftStadiumCenter = this.tennisStadiumCenter;

    // 3개 경기장 전체가 여유롭게 프레임에 안착하는 와이드 시네마틱 앵글
    this.skyCamPos = new THREE.Vector3(25, 205, 435);
    this.skyLookAt = new THREE.Vector3(-30, 20, -10);

    this.currentCamPos = new THREE.Vector3().copy(this.skyCamPos);
    this.targetCamPos = new THREE.Vector3().copy(this.skyCamPos);
    this.currentLookAt = new THREE.Vector3().copy(this.skyLookAt);
    this.targetLookAt = new THREE.Vector3().copy(this.skyLookAt);

    this.scrollProgress = 0;
    this.particles = null;
    this.balloonsSystem = null;
    this.fireworksSystem = null;

    // Time of Day (시간대) 상태 관리
    this.currentTod = 'day'; // 'day' | 'sunset' | 'night'
    this.hemiLight = null;
    this.sunLight = null;
    this.fillLight = null;
    this.skyCanvas = null;
    this.skyCtx = null;
    this.skyTexture = null;

    // 경기장 투광 조명 및 은은한 렌즈 글로우 스프라이트 리스트
    this.stadiumFloodLights = [];
    this.stadiumGlowSprites = [];
    this.stadiumBulbMaterial = null;
    this.stadiumLightMaterials = [];

    this.initScene();
    this.initEmotionalSky();
    this.initVastOceanAndMegaIsland();
    this.initAtmosphereParticles();
    this.initLights();
    this.loadThreeStadiums();
    this.initEventListeners();

    this.animate = this.animate.bind(this);
    requestAnimationFrame(this.animate);
  }

  initScene() {
    this.scene = new THREE.Scene();
    this.scene.add(this.islandGroup);

    // 닌텐도 스위치 스포츠 감성의 청량하고 부드러운 파스텔 원경 안개
    this.scene.fog = new THREE.Fog(0xbde8ff, 450, 2400);

    const width = window.innerWidth;
    const height = window.innerHeight;

    this.camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 5000);
    this.camera.position.copy(this.skyCamPos);
    this.camera.lookAt(this.skyLookAt);

    this.renderer = new THREE.WebGLRenderer({ antialias: true, powerPreference: 'high-performance' });
    this.renderer.setSize(width, height);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.toneMappingExposure = 1.28;

    this.container.innerHTML = '';
    this.container.appendChild(this.renderer.domElement);
  }

  /* 닌텐도 스위치 스포츠 감성 + 골든 아워 샴페인 틴트 시네마틱 스카이 */
  initEmotionalSky() {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d');

    const grad = ctx.createLinearGradient(0, 0, 0, 512);
    grad.addColorStop(0, '#004ea8');    // Deep Vivid Nintendo Cobalt
    grad.addColorStop(0.32, '#0284c7'); // Mediterranean Azure
    grad.addColorStop(0.65, '#38bdf8'); // Sparkling Tropical Cyan
    grad.addColorStop(0.86, '#bae6fd'); // Soft Pastel Horizon
    grad.addColorStop(0.94, '#fed7aa'); // Warm Golden Sunset Tint
    grad.addColorStop(1, '#ffffff');    // Brilliant Horizon

    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, 512, 512);

    this.skyCanvas = canvas;
    this.skyCtx = ctx;
    this.skyTexture = new THREE.CanvasTexture(canvas);
    this.scene.background = this.skyTexture;

    // 푹신푹신한 볼륨감의 닌텐도 카툰 솜사탕 구름 클러스터
    this.cloudsGroup = new THREE.Group();
    const cloudCount = 22;

    for (let i = 0; i < cloudCount; i++) {
      const cloud = createStylizedCloud();
      const angle = (i / cloudCount) * Math.PI * 2 + (Math.random() - 0.5) * 0.2;
      const radius = 560 + Math.random() * 260;
      const height = 110 + Math.random() * 85;

      cloud.position.set(
        Math.cos(angle) * radius,
        height,
        Math.sin(angle) * radius
      );
      const scale = 2.2 + Math.random() * 1.6;
      cloud.scale.set(scale, scale * 0.75, scale * 1.1);
      this.cloudsGroup.add(cloud);
    }
    this.scene.add(this.cloudsGroup);
  }

  /* 화사하고 생생한 닌텐도 리조트 메가 아일랜드 & 맑은 바다 & 산호초 군도 */
  initVastOceanAndMegaIsland() {
    // 1. 에메랄드빛 카툰 수면 텍스처 바다
    const oceanGeo = new THREE.PlaneGeometry(10000, 10000, 64, 64);
    const waterTex = createResortWaterTexture();
    const oceanMat = new THREE.MeshStandardMaterial({
      color: 0x0284c7,
      map: waterTex,
      roughness: 0.12,
      metalness: 0.15,
      transparent: true,
      opacity: 0.95
    });
    this.waterMesh = new THREE.Mesh(oceanGeo, oceanMat);
    this.waterMesh.rotation.x = -Math.PI / 2;
    this.waterMesh.position.y = -0.8;
    this.scene.add(this.waterMesh);

    // 유기적인 곡선 섬 외곽 형태를 생성하는 헬퍼
    const createOrganicShape = (baseRadius, noiseScale = 0.12, pointsCount = 120, seed = 0) => {
      const shape = new THREE.Shape();
      for (let i = 0; i <= pointsCount; i++) {
        const theta = (i / pointsCount) * Math.PI * 2;
        const wave = 
          Math.sin(theta * 3 + seed) * 0.18 +
          Math.cos(theta * 5 + seed * 1.5) * 0.10 +
          Math.sin(theta * 7 - seed) * 0.05;
        const r = baseRadius * (1 + wave * noiseScale);
        const x = Math.cos(theta) * r;
        const y = Math.sin(theta) * r;
        if (i === 0) shape.moveTo(x, y);
        else shape.lineTo(x, y);
      }
      return shape;
    };

    // 2. 바다 밑 얕은 터콰이즈 산호초 림 (해안가 에메랄드 워터 엣지)
    const reefShape = createOrganicShape(445, 0.22, 140, 1);
    const reefGeo = new THREE.ShapeGeometry(reefShape);
    const reefMat = new THREE.MeshStandardMaterial({
      color: 0x22d3ee,
      roughness: 0.35,
      metalness: 0.1,
      transparent: true,
      opacity: 0.72
    });
    const reefMesh = new THREE.Mesh(reefGeo, reefMat);
    reefMesh.rotation.x = -Math.PI / 2;
    reefMesh.position.y = -0.45;
    this.islandGroup.add(reefMesh);

    // 3. 햇살 가득한 황금빛 모래사장 비치 (반지름 410)
    const sandShape = createOrganicShape(410, 0.18, 140, 2);
    const sandGeo = new THREE.ShapeGeometry(sandShape);
    const sandMat = new THREE.MeshStandardMaterial({
      color: 0xfef08a,
      roughness: 0.85,
      metalness: 0.05
    });
    const sandMesh = new THREE.Mesh(sandGeo, sandMat);
    sandMesh.rotation.x = -Math.PI / 2;
    sandMesh.position.y = -0.15;
    sandMesh.receiveShadow = true;
    this.islandGroup.add(sandMesh);

    // 4. 싱그러운 닌텐도 2톤 체커보드 메가 잔디 평원 (반지름 385)
    const grassShape = createOrganicShape(385, 0.15, 140, 3);
    const grassGeo = new THREE.ShapeGeometry(grassShape);
    const grassTex = createNintendoGrassTexture();
    const grassMat = new THREE.MeshStandardMaterial({
      map: grassTex,
      roughness: 0.78,
      metalness: 0.05
    });
    const grassMesh = new THREE.Mesh(grassGeo, grassMat);
    grassMesh.rotation.x = -Math.PI / 2;
    grassMesh.position.y = 0.0;
    grassMesh.receiveShadow = true;
    this.islandGroup.add(grassMesh);

    // 5. 주변을 둘러싼 아기자기한 열대 부속 섬 5개
    const subIslands = [
      { x: -440, z: -260, r: 80 },
      { x: -390, z: 310, r: 95 },
      { x: 380, z: -320, r: 105 },
      { x: 430, z: 270, r: 85 },
      { x: 20, z: 460, r: 75 },
    ];

    subIslands.forEach((isle, idx) => {
      const subCoral = new THREE.Mesh(
        new THREE.ShapeGeometry(createOrganicShape(isle.r * 1.35, 0.15, 60, idx + 2)),
        reefMat
      );
      subCoral.rotation.x = -Math.PI / 2;
      subCoral.position.set(isle.x, -0.6, isle.z);
      this.islandGroup.add(subCoral);

      const subSand = new THREE.Mesh(
        new THREE.ShapeGeometry(createOrganicShape(isle.r * 1.15, 0.12, 60, idx + 4)),
        sandMat
      );
      subSand.rotation.x = -Math.PI / 2;
      subSand.position.set(isle.x, -0.3, isle.z);
      this.islandGroup.add(subSand);

      const subGrass = new THREE.Mesh(
        new THREE.ShapeGeometry(createOrganicShape(isle.r, 0.1, 60, idx + 6)),
        grassMat
      );
      subGrass.rotation.x = -Math.PI / 2;
      subGrass.position.set(isle.x, -0.05, isle.z);
      subGrass.receiveShadow = true;
      this.islandGroup.add(subGrass);
    });

    // 6. 닌텐도 카툰풍 열대 야자수 배치
    const trunkGeo = new THREE.CylinderGeometry(0.8, 1.3, 8.5, 8);
    const trunkMat = new THREE.MeshStandardMaterial({ color: 0x92400e, roughness: 0.75 });
    const leavesLowerGeo = new THREE.SphereGeometry(4.8, 8, 8);
    const leavesUpperGeo = new THREE.SphereGeometry(3.6, 8, 8);
    const leavesMat = new THREE.MeshStandardMaterial({ color: 0x22c55e, roughness: 0.5 });
    const leavesTopMat = new THREE.MeshStandardMaterial({ color: 0x4ade80, roughness: 0.5 });

    for (let i = 0; i < 110; i++) {
      const angle = Math.random() * Math.PI * 2;
      const r = 210 + Math.random() * 165;
      const tx = Math.cos(angle) * r;
      const tz = Math.sin(angle) * r;

      // 돔 스타디움과의 충돌 방지
      const distToLeft = Math.hypot(tx - (-160), tz);
      const distToTopRight = Math.hypot(tx - 130, tz - (-150));
      const distToBottomRight = Math.hypot(tx - 130, tz - 150);

      if (distToLeft < 98 || distToTopRight < 95 || distToBottomRight < 95) continue;

      const tree = new THREE.Group();
      const trunk = new THREE.Mesh(trunkGeo, trunkMat);
      trunk.position.y = 4.25;
      trunk.rotation.z = (Math.random() - 0.5) * 0.16;

      const leavesLower = new THREE.Mesh(leavesLowerGeo, leavesMat);
      leavesLower.position.y = 9.2;
      leavesLower.scale.set(1.1, 0.65, 1.1);

      const leavesUpper = new THREE.Mesh(leavesUpperGeo, leavesTopMat);
      leavesUpper.position.y = 11.5;
      leavesUpper.scale.set(0.9, 0.75, 0.9);

      tree.add(trunk);
      tree.add(leavesLower);
      tree.add(leavesUpper);
      tree.position.set(tx, 0, tz);
      this.islandGroup.add(tree);
    }

    // 7. 먼 바다의 귀여운 리조트 세일보트 (요트)
    this.boatsGroup = new THREE.Group();
    const boatHullGeo = new THREE.BoxGeometry(7, 3, 14);
    const boatHullMat = new THREE.MeshStandardMaterial({ color: 0xffffff, roughness: 0.3 });
    const boatSailGeo = new THREE.ConeGeometry(5, 12, 3);
    const boatSailMat = new THREE.MeshStandardMaterial({ color: 0xff5533, roughness: 0.4 });

    const boatCoords = [
      { x: -320, z: 360, rot: 0.5 },
      { x: 380, z: -80, rot: -1.2 },
      { x: -80, z: -420, rot: 2.1 }
    ];

    boatCoords.forEach((bc) => {
      const boat = new THREE.Group();
      const hull = new THREE.Mesh(boatHullGeo, boatHullMat);
      hull.position.y = 0.5;
      const sail = new THREE.Mesh(boatSailGeo, boatSailMat);
      sail.position.set(0, 7.5, 0);
      sail.scale.set(0.3, 1, 1);
      boat.add(hull);
      boat.add(sail);
      boat.position.set(bc.x, -0.2, bc.z);
      boat.rotation.y = bc.rot;
      this.boatsGroup.add(boat);
    });
    this.scene.add(this.boatsGroup);
  }

  initAtmosphereParticles() {
    // 1. 닌텐도 스위치 스포츠 감성 알록달록 하늘 풍선 (Floating Balloons)
    this.balloonsSystem = createFloatingBalloonsGroup();
    this.islandGroup.add(this.balloonsSystem.group);

    // 2. 하늘에 쉼 없이 펑펑 터지는 축제 폭죽 (Continuous Fireworks)
    this.fireworksSystem = createFestiveFireworksSystem();
    this.scene.add(this.fireworksSystem.group);
  }

  /* 닌텐도 스위치 스포츠 감성: 화사한 앰비언트 스카이라이트 & 쨍한 선샤인 라이팅 */
  initLights() {
    const hemiLight = new THREE.HemisphereLight(0x7dd3fc, 0x86efac, 1.5);
    this.scene.add(hemiLight);
    this.hemiLight = hemiLight;

    const sunLight = new THREE.DirectionalLight(0xfff5e6, 2.9);
    sunLight.position.set(220, 380, 260);
    sunLight.castShadow = true;
    sunLight.shadow.mapSize.width = 2048;
    sunLight.shadow.mapSize.height = 2048;
    sunLight.shadow.camera.near = 50;
    sunLight.shadow.camera.far = 850;
    sunLight.shadow.bias = -0.0006;
    sunLight.shadow.normalBias = 0.05;
    const d = 320;
    sunLight.shadow.camera.left = -d;
    sunLight.shadow.camera.right = d;
    sunLight.shadow.camera.top = d;
    sunLight.shadow.camera.bottom = -d;
    this.scene.add(sunLight);
    this.sunLight = sunLight;

    const fillLight = new THREE.DirectionalLight(0x38bdf8, 0.95);
    fillLight.position.set(-220, 160, -220);
    this.scene.add(fillLight);
    this.fillLight = fillLight;

    // 조명 전구 발광 머티리얼
    this.stadiumBulbMaterial = new THREE.MeshStandardMaterial({
      color: 0xffffff,
      emissive: new THREE.Color(0xffffff),
      emissiveIntensity: 0.1,
      roughness: 0.1,
      metalness: 0.1
    });
  }

  /* =========================================================================
     스타디움 조명 타워 & 렌즈 글로우 스프라이트 (Fake Glow Sprite)
     - 부자연스러운 스팟라이트/빛기둥/바닥 스팟 경계선 제거!
     - 램프 헤드에 은은한 글로우 스프라이트로 실제 불빛이 켜진 듯한 자연스러운 효과
     - 부드러운 무지향성 주변광(PointLight)으로 관중석과 코트를 화사하게 유지
  ========================================================================= */
  addStadiumLightingSystem(stadiumUnit) {
    const towerConfigs = [
      { x: -24, z: -33, rotY: Math.PI * 0.25 },
      { x:  24, z: -33, rotY: -Math.PI * 0.25 },
      { x: -24, z:  33, rotY: Math.PI * 0.75 },
      { x:  24, z:  33, rotY: -Math.PI * 0.75 },
    ];

    const poleGeo = new THREE.CylinderGeometry(0.38, 0.62, 18, 12);
    const metalMat = new THREE.MeshStandardMaterial({
      color: 0x334155,
      roughness: 0.35,
      metalness: 0.75
    });

    const headGeo = new THREE.BoxGeometry(4.4, 2.2, 1.3);
    const headMat = new THREE.MeshStandardMaterial({
      color: 0x1e293b,
      roughness: 0.3,
      metalness: 0.8
    });

    const visorGeo = new THREE.BoxGeometry(4.6, 0.35, 1.6);
    const visorMat = new THREE.MeshStandardMaterial({
      color: 0x0f172a,
      roughness: 0.2,
      metalness: 0.9
    });

    const bulbGeo = new THREE.SphereGeometry(0.55, 12, 12);
    const glowTex = createGlowSpriteTexture();

    towerConfigs.forEach((cfg) => {
      const tower = new THREE.Group();
      tower.position.set(cfg.x, 8, cfg.z);
      tower.rotation.y = cfg.rotY;

      // 1. 견고한 수직 라이트 마운트 폴 (y: 0 ~ 18)
      const pole = new THREE.Mesh(poleGeo, metalMat);
      pole.position.y = 9;
      pole.castShadow = true;
      tower.add(pole);

      // 2. 조명 타워 헤드 (y: 18 위치, 코트 안쪽을 향해 38도 아래로 숙임)
      const headGroup = new THREE.Group();
      headGroup.position.y = 18;
      headGroup.rotation.x = 0.38;

      const headBox = new THREE.Mesh(headGeo, headMat);
      headGroup.add(headBox);

      // 상단 바이저 (차광판 엣지 캡)
      const visor = new THREE.Mesh(visorGeo, visorMat);
      visor.position.set(0, 1.15, 0.15);
      headGroup.add(visor);

      // 3. 고휘도 4구 LED 램프 전구
      const bulbPositions = [
        { x: -1.3, y: 0.45 },
        { x:  1.3, y: 0.45 },
        { x: -1.3, y: -0.45 },
        { x:  1.3, y: -0.45 },
      ];

      bulbPositions.forEach((bp) => {
        const bulb = new THREE.Mesh(bulbGeo, this.stadiumBulbMaterial);
        bulb.position.set(bp.x, bp.y, 0.65);
        headGroup.add(bulb);
      });

      // 4. 부드러운 옴니 주변광 PointLight (경계선 없는 자연스러운 광원)
      const floodLight = new THREE.PointLight(0xfffae6, 0.05, 110, 1.2);
      floodLight.position.set(0, 0, 1.5);
      headGroup.add(floodLight);
      this.stadiumFloodLights.push(floodLight);

      // 5. ★★★ 렌즈 글로우 스프라이트 (Fake Glow Sprite) ★★★
      // 카메라 각도에 상관없이 조명 램프가 영롱하게 켜진 느낌을 주는 부드러운 글로우
      const glowMat = new THREE.SpriteMaterial({
        map: glowTex,
        color: 0xffffff,
        transparent: true,
        opacity: 0.0,
        blending: THREE.AdditiveBlending,
        depthWrite: false
      });
      const glowSprite = new THREE.Sprite(glowMat);
      glowSprite.position.set(0, 0, 1.8);
      glowSprite.scale.set(6.0, 6.0, 1.0);
      headGroup.add(glowSprite);
      this.stadiumGlowSprites.push(glowMat);

      tower.add(headGroup);
      stadiumUnit.add(tower);
    });
  }

  /* 시간대 (Time of Day) 3단 순환 전환 시스템: day ➔ sunset ➔ night ➔ day */
  cycleTimeOfDay() {
    const modes = ['day', 'sunset', 'night'];
    const nextIdx = (modes.indexOf(this.currentTod) + 1) % modes.length;
    this.setTimeOfDay(modes[nextIdx]);
    return this.currentTod;
  }

  setTimeOfDay(mode = 'day') {
    this.currentTod = mode;

    // 1. 스카이 캔버스 그라디언트 갱신
    if (this.skyCanvas && this.skyCtx && this.skyTexture) {
      const ctx = this.skyCtx;
      const grad = ctx.createLinearGradient(0, 0, 0, 512);

      if (mode === 'day') {
        grad.addColorStop(0, '#004ea8');    // Deep Vivid Nintendo Cobalt
        grad.addColorStop(0.32, '#0284c7'); // Mediterranean Azure
        grad.addColorStop(0.65, '#38bdf8'); // Sparkling Tropical Cyan
        grad.addColorStop(0.86, '#bae6fd'); // Soft Pastel Horizon
        grad.addColorStop(0.94, '#fed7aa'); // Warm Golden Sunset Tint
        grad.addColorStop(1, '#ffffff');    // Brilliant Horizon
      } else if (mode === 'sunset') {
        grad.addColorStop(0, '#2e0854');    // Deep Twilight Violet
        grad.addColorStop(0.25, '#581c87'); // Royal Purple
        grad.addColorStop(0.52, '#c026d3'); // Radiant Magenta Sunset
        grad.addColorStop(0.76, '#ea580c'); // Fiery Orange
        grad.addColorStop(0.90, '#f59e0b'); // Golden Amber Horizon
        grad.addColorStop(1, '#fef08a');    // Glowing Warm Sunlight
      } else if (mode === 'night') {
        grad.addColorStop(0, '#020617');    // Deep Space Midnight Navy
        grad.addColorStop(0.28, '#0f172a'); // Rich Slate Midnight
        grad.addColorStop(0.58, '#1e1b4b'); // Electric Indigo Night
        grad.addColorStop(0.82, '#0284c7'); // Vibrant Cyberpunk Horizon Glow
        grad.addColorStop(1, '#38bdf8');    // Neon Cyan Horizon Rim
      }

      ctx.fillStyle = grad;
      ctx.fillRect(0, 0, 512, 512);
      this.skyTexture.needsUpdate = true;
    }

    // 2. 환경 조명 & 앰비언스 & 포그 갱신
    if (this.hemiLight && this.sunLight && this.fillLight) {
      if (mode === 'day') {
        this.hemiLight.color.setHex(0x7dd3fc);
        this.hemiLight.groundColor.setHex(0x86efac);
        this.hemiLight.intensity = 1.5;

        this.sunLight.color.setHex(0xfff5e6);
        this.sunLight.intensity = 2.9;

        this.fillLight.color.setHex(0x38bdf8);
        this.fillLight.intensity = 0.95;

        if (this.scene.fog) this.scene.fog.color.setHex(0xbde8ff);
        if (this.renderer) this.renderer.toneMappingExposure = 1.28;
        if (this.waterMesh) this.waterMesh.material.color.setHex(0x0284c7);
      } else if (mode === 'sunset') {
        this.hemiLight.color.setHex(0xf472b6);
        this.hemiLight.groundColor.setHex(0xd97706);
        this.hemiLight.intensity = 1.55;

        this.sunLight.color.setHex(0xff7033);
        this.sunLight.intensity = 3.3;

        this.fillLight.color.setHex(0xc084fc);
        this.fillLight.intensity = 1.2;

        if (this.scene.fog) this.scene.fog.color.setHex(0xfbcfe8);
        if (this.renderer) this.renderer.toneMappingExposure = 1.35;
        if (this.waterMesh) this.waterMesh.material.color.setHex(0x0369a1);
      } else if (mode === 'night') {
        this.hemiLight.color.setHex(0x60a5fa);
        this.hemiLight.groundColor.setHex(0x1e3a5f);
        this.hemiLight.intensity = 1.35;

        this.sunLight.color.setHex(0x93c5fd);
        this.sunLight.intensity = 2.5;

        this.fillLight.color.setHex(0x818cf8);
        this.fillLight.intensity = 1.15;

        if (this.scene.fog) this.scene.fog.color.setHex(0x0b1736);
        if (this.renderer) this.renderer.toneMappingExposure = 1.38;
        if (this.waterMesh) this.waterMesh.material.color.setHex(0x021f4a);
      }
    }

    // 3. 조명탑 주변광 & 렌즈 글로우 스프라이트 제어
    if (mode === 'day') {
      this.stadiumFloodLights.forEach((light) => {
        light.color.setHex(0xfffae6);
        light.intensity = 0.05;
        light.distance = 90;
      });
      this.stadiumGlowSprites.forEach((mat) => {
        mat.opacity = 0.0;
      });
    } else if (mode === 'sunset') {
      this.stadiumFloodLights.forEach((light) => {
        light.color.setHex(0xffaa44);
        light.intensity = 0.35;
        light.distance = 120;
      });
      this.stadiumGlowSprites.forEach((mat) => {
        mat.color.setHex(0xffaa44);
        mat.opacity = 0.45; // 노을 앰버빛 은은한 글로우
      });
    } else if (mode === 'night') {
      this.stadiumFloodLights.forEach((light) => {
        light.color.setHex(0xffffff);
        light.intensity = 0.55;
        light.distance = 135;
      });
      this.stadiumGlowSprites.forEach((mat) => {
        mat.color.setHex(0xffffff);
        mat.opacity = 0.70; // 밤하늘에 반짝이는 영롱한 화이트 글로우
      });
    }

    // 4. 고휘도 LED 전구 머티리얼 발광 조율
    if (this.stadiumBulbMaterial) {
      if (mode === 'day') {
        this.stadiumBulbMaterial.emissive.setHex(0xffffff);
        this.stadiumBulbMaterial.emissiveIntensity = 0.1;
      } else if (mode === 'sunset') {
        this.stadiumBulbMaterial.emissive.setHex(0xffa834);
        this.stadiumBulbMaterial.emissiveIntensity = 0.35;
      } else if (mode === 'night') {
        this.stadiumBulbMaterial.emissive.setHex(0xffffff);
        this.stadiumBulbMaterial.emissiveIntensity = 0.50;
      }
    }

    // 5. FBX 모델 내 Light 메쉬 머티리얼 발광 조율
    this.stadiumLightMaterials.forEach((mat) => {
      if (mode === 'day') {
        mat.emissive.setHex(0xfffae6);
        mat.emissiveIntensity = 0.1;
      } else if (mode === 'sunset') {
        mat.emissive.setHex(0xff9922);
        mat.emissiveIntensity = 0.35;
      } else if (mode === 'night') {
        mat.emissive.setHex(0xffffff);
        mat.emissiveIntensity = 0.45;
      }
    });

    // 6. 돔 LED 링 발광 색상 조율
    this.islandGroup.traverse((child) => {
      if (child.isMesh) {
        const name = (child.name || '').toLowerCase();
        if (name.includes('stadium_led_ring')) {
          if (mode === 'day') child.material.color.setHex(0x38bdf8);
          else if (mode === 'sunset') child.material.color.setHex(0xfbbf24);
          else if (mode === 'night') child.material.color.setHex(0x00f5ff);
        }
      }
    });
  }

  /* 3대 경기장 배치 및 로드 */
  loadThreeStadiums() {
    const fbxLoader = new FBXLoader();

    const courtTex = this.textureLoader.load('assets/models/court/Texture/Tennis__Tennis_AlbedoTransparency.png');
    const chairMainTex = this.textureLoader.load('assets/models/court/Texture/Tennis__Chair_Main1_AlbedoTransparency.png');
    const chairRearTex = this.textureLoader.load('assets/models/court/Texture/Tennis__Chair_rear_AlbedoTransparency.png');
    const extraTex = this.textureLoader.load('assets/models/court/Texture/Tennis__Extra1_AlbedoTransparency.png');

    const back1Tex = this.textureLoader.load('assets/models/court/Texture/Tennis_Back_Back_1_AlbedoTransparency.png');
    const back2Tex = this.textureLoader.load('assets/models/court/Texture/Tennis_Back_Back_2_AlbedoTransparency.png');
    const lightTex = this.textureLoader.load('assets/models/court/Texture/Tennis_Back_Light_AlbedoTransparency.png');
    const opTex = this.textureLoader.load('assets/models/court/Texture/Tennis_Back_Back_1_OP_AlbedoTransparency.png');

    const buildStadiumUnit = (onReady) => {
      const stadiumUnit = new THREE.Group();

      fbxLoader.load('assets/models/court/Tennis_.fbx', (courtFbx) => {
        courtFbx.rotation.y = Math.PI / 2;
        courtFbx.traverse((child) => {
          if (child.isMesh) {
            child.receiveShadow = true;
            const name = (child.name || '').toLowerCase();
            const isFloor = !name.includes('chair') && !name.includes('extra') && !name.includes('net') && !name.includes('post');
            child.castShadow = !isFloor;

            if (name.includes('chair_main')) {
              child.material = new THREE.MeshStandardMaterial({ map: chairMainTex, roughness: 0.5, side: THREE.DoubleSide });
            } else if (name.includes('chair_rear')) {
              child.material = new THREE.MeshStandardMaterial({ map: chairRearTex, roughness: 0.5, side: THREE.DoubleSide });
            } else if (name.includes('extra')) {
              child.material = new THREE.MeshStandardMaterial({ map: extraTex, roughness: 0.5, side: THREE.DoubleSide });
            } else {
              child.material = new THREE.MeshStandardMaterial({
                map: courtTex,
                roughness: 0.65,
                side: THREE.DoubleSide,
                polygonOffset: true,
                polygonOffsetFactor: -1.0,
                polygonOffsetUnits: -1.0
              });
            }
          }
        });
        const box = new THREE.Box3().setFromObject(courtFbx);
        const center = box.getCenter(new THREE.Vector3());
        courtFbx.position.x -= center.x;
        courtFbx.position.y -= box.min.y;
        courtFbx.position.y += 0.06; // 코트 바닥이 아래층 바닥/지형과 겹치거나 Z-fighting 생기지 않도록 미세 부양
        courtFbx.position.z -= center.z;
        stadiumUnit.add(courtFbx);

        fbxLoader.load('assets/models/court/Tennis_Back.fbx', (standsFbx) => {
          standsFbx.traverse((child) => {
            if (child.isMesh) {
              child.castShadow = true;
              child.receiveShadow = true;
              const name = (child.name || '').toLowerCase();
              const matName = (child.material && child.material.name ? child.material.name : '').toLowerCase();
              const isLight = name.includes('light') || matName.includes('light');

              if (isLight) {
                const lightMat = new THREE.MeshStandardMaterial({
                  map: lightTex,
                  emissive: new THREE.Color(0xfffae6),
                  emissiveIntensity: 0.35,
                  roughness: 0.2,
                  metalness: 0.1,
                  side: THREE.DoubleSide
                });
                child.material = lightMat;
                this.stadiumLightMaterials.push(lightMat);
              } else if (name.includes('back_2') || name.includes('back2')) {
                child.material = new THREE.MeshStandardMaterial({ map: back2Tex, roughness: 0.6, side: THREE.DoubleSide });
              } else if (name.includes('op')) {
                child.material = new THREE.MeshStandardMaterial({ map: opTex, transparent: true, alphaTest: 0.2, side: THREE.DoubleSide });
              } else {
                child.material = new THREE.MeshStandardMaterial({ map: back1Tex, roughness: 0.6, side: THREE.DoubleSide });
              }
            }
          });
          // 경기장 내부(관중석: Tennis_Back.fbx) 90도 회전 및 사이즈 약간 확대
          standsFbx.rotation.y = Math.PI / 2;
          standsFbx.scale.set(1.08, 1.05, 1.08);

          const b = new THREE.Box3().setFromObject(standsFbx);
          const c = b.getCenter(new THREE.Vector3());
          standsFbx.position.x -= c.x;
          standsFbx.position.y -= b.min.y;
          standsFbx.position.z -= c.z;
          stadiumUnit.add(standsFbx);

          // 1. ACE DOME: 거대 돔형 외벽 껍데기 래퍼
          addGrandAceDomeShell(stadiumUnit);

          // 2. 스타디움 조명 타워 & 렌즈 글로우 스프라이트 장착
          this.addStadiumLightingSystem(stadiumUnit);

          onReady(stadiumUnit);
        });
      });
    };

    // 1. Left Stadium (Main focus stadium at x: -160, y: 1.2, z: 0)
    buildStadiumUnit((unit) => {
      this.leftStadiumGroup.add(unit);
      this.leftStadiumGroup.position.copy(this.leftStadiumCenter);
      this.islandGroup.add(this.leftStadiumGroup);

      const preloader = document.getElementById('initial-preloader');
      if (preloader) {
        preloader.classList.add('fade-out');
        setTimeout(() => preloader.remove(), 800);
      }
    });

    // 2. Top-Right Stadium (Bowling at x: 130, y: 1.2, z: -150 - 테니스와 동일한 y: 1.2 높이로 잔디 바닥 겹침 제거)
    buildStadiumUnit((unit) => {
      this.topRightStadiumGroup.add(unit);
      this.topRightStadiumGroup.position.copy(this.bowlingStadiumCenter);
      this.topRightStadiumGroup.rotation.y = -Math.PI / 4;
      this.islandGroup.add(this.topRightStadiumGroup);
    });

    // 3. Bottom-Right Stadium (Swordplay at x: 130, y: 1.2, z: 150 - 테니스와 동일한 y: 1.2 높이로 잔디 바닥 겹침 제거)
    buildStadiumUnit((unit) => {
      this.bottomRightStadiumGroup.add(unit);
      this.bottomRightStadiumGroup.position.copy(this.swordStadiumCenter);
      this.bottomRightStadiumGroup.rotation.y = Math.PI / 4;
      this.islandGroup.add(this.bottomRightStadiumGroup);
    });
  }

  /* =========================================================================
     3대 경기장 순차 완결 투어 시네마틱 카메라 궤적:
     - Step 0 (p: 0.00 ~ 0.04): 첫 화면 와이드 스카이뷰 (낮)
     - Step 1 ~ 3 (p: 0.04 ~ 0.28): 🎾 테니스 경기장 내부 3스텝 완결 공전 (낮)
     - 전환 1 (p: 0.28 ~ 0.40): 테니스 ➔ 볼링 경기장 상공 완만한 시네마틱 활공 (노을로 전환)
     - Step 4 ~ 6 (p: 0.40 ~ 0.62): 🎳 볼링 경기장 내부 3스텝 완결 공전 (노을)
     - 전환 2 (p: 0.62 ~ 0.74): 볼링 ➔ 검술 경기장 상공 완만한 시네마틱 활공 (밤으로 전환)
     - Step 7 ~ 9 (p: 0.74 ~ 0.92): ⚔️ 검술 경기장 내부 3스텝 완결 공전 (밤)
     - 전환 3 (p: 0.92 ~ 1.00): 검술 ➔ 3대 경기장 전체 조망 항공뷰로 완만하게 상승
     - Step 10 (p: 1.00): 🏆 3대 경기장 전체 조망 항공뷰 피날레 (밤)
  ========================================================================= */
  updateCameraForScroll(p) {
    this.scrollProgress = Math.max(0, Math.min(1, p));

    // 1. 오프닝 스카이뷰 -> 테니스 돔 진입 (p: 0.00 ~ 0.04)
    if (this.scrollProgress <= 0.04) {
      const f = this.scrollProgress / 0.04;
      this.targetCamPos.lerpVectors(this.skyCamPos, new THREE.Vector3(-148, 48, 40), f);
      this.targetLookAt.lerpVectors(this.skyLookAt, this.tennisStadiumCenter, f);
    }
    // 2. 🎾 테니스 경기장 내부 완결 공전 (Step 1 ~ 3, p: 0.04 ~ 0.28)
    else if (this.scrollProgress <= 0.28) {
      const t = (this.scrollProgress - 0.04) / 0.24;
      const angle = t * Math.PI * 1.5 - 0.2;
      const radius = 24.5 - t * 2.5;
      const height = 7.8 + Math.sin(t * Math.PI) * 3.8;

      const camX = this.tennisStadiumCenter.x + Math.sin(angle) * radius;
      const camZ = this.tennisStadiumCenter.z + Math.cos(angle) * radius;

      this.targetCamPos.set(camX, height, camZ);
      this.targetLookAt.copy(this.tennisStadiumCenter);
    }
    // 3. 전환 1: 테니스 ➔ 볼링 상공 부드러운 곡선 활공 비행 (p: 0.28 ~ 0.40)
    else if (this.scrollProgress <= 0.40) {
      const rawT = (this.scrollProgress - 0.28) / 0.12;
      const s = rawT * rawT * (3 - 2 * rawT); // Smooth cubic ease

      // 완만한 2차 베지어 곡선으로 리조트 중앙 만(bay) 위를 우아하게 활공
      const p0 = new THREE.Vector3(-176, 14, 15);
      const p1 = new THREE.Vector3(-20, 78, -75);
      const p2 = new THREE.Vector3(120, 36, -135);

      const oneMinusS = 1 - s;
      const posX = oneMinusS * oneMinusS * p0.x + 2 * oneMinusS * s * p1.x + s * s * p2.x;
      const posY = oneMinusS * oneMinusS * p0.y + 2 * oneMinusS * s * p1.y + s * s * p2.y;
      const posZ = oneMinusS * oneMinusS * p0.z + 2 * oneMinusS * s * p1.z + s * s * p2.z;
      this.targetCamPos.set(posX, posY, posZ);

      const look0 = this.tennisStadiumCenter;
      const look1 = new THREE.Vector3(0, 15, -75);
      const look2 = this.bowlingStadiumCenter;
      const lookX = oneMinusS * oneMinusS * look0.x + 2 * oneMinusS * s * look1.x + s * s * look2.x;
      const lookY = oneMinusS * oneMinusS * look0.y + 2 * oneMinusS * s * look1.y + s * s * look2.y;
      const lookZ = oneMinusS * oneMinusS * look0.z + 2 * oneMinusS * s * look1.z + s * s * look2.z;
      this.targetLookAt.set(lookX, lookY, lookZ);
    }
    // 4. 🎳 볼링 경기장 내부 완결 공전 (Step 4 ~ 6, p: 0.40 ~ 0.62)
    else if (this.scrollProgress <= 0.62) {
      const t = (this.scrollProgress - 0.40) / 0.22;
      const angle = t * Math.PI * 1.5 - Math.PI * 0.35;
      const radius = 25.0 - t * 2.0;
      const height = 8.0 + Math.sin(t * Math.PI) * 3.6;

      const camX = this.bowlingStadiumCenter.x + Math.sin(angle) * radius;
      const camZ = this.bowlingStadiumCenter.z + Math.cos(angle) * radius;

      this.targetCamPos.set(camX, height, camZ);
      this.targetLookAt.copy(this.bowlingStadiumCenter);
    }
    // 5. 전환 2: 볼링 ➔ 검술 상공 부드러운 곡선 활공 비행 (p: 0.62 ~ 0.74)
    else if (this.scrollProgress <= 0.74) {
      const rawT = (this.scrollProgress - 0.62) / 0.12;
      const s = rawT * rawT * (3 - 2 * rawT);

      // 동쪽 해안선과 야경을 따라 우아하게 비행
      const p0 = new THREE.Vector3(144, 15, -135);
      const p1 = new THREE.Vector3(180, 80, 0);
      const p2 = new THREE.Vector3(122, 36, 135);

      const oneMinusS = 1 - s;
      const posX = oneMinusS * oneMinusS * p0.x + 2 * oneMinusS * s * p1.x + s * s * p2.x;
      const posY = oneMinusS * oneMinusS * p0.y + 2 * oneMinusS * s * p1.y + s * s * p2.y;
      const posZ = oneMinusS * oneMinusS * p0.z + 2 * oneMinusS * s * p1.z + s * s * p2.z;
      this.targetCamPos.set(posX, posY, posZ);

      const look0 = this.bowlingStadiumCenter;
      const look1 = new THREE.Vector3(145, 12, 0);
      const look2 = this.swordStadiumCenter;
      const lookX = oneMinusS * oneMinusS * look0.x + 2 * oneMinusS * s * look1.x + s * s * look2.x;
      const lookY = oneMinusS * oneMinusS * look0.y + 2 * oneMinusS * s * look1.y + s * s * look2.y;
      const lookZ = oneMinusS * oneMinusS * look0.z + 2 * oneMinusS * s * look1.z + s * s * look2.z;
      this.targetLookAt.set(lookX, lookY, lookZ);
    }
    // 6. ⚔️ 검술 경기장 내부 완결 공전 (Step 7 ~ 9, p: 0.74 ~ 0.92)
    else if (this.scrollProgress <= 0.92) {
      const t = (this.scrollProgress - 0.74) / 0.18;
      const angle = t * Math.PI * 1.5 + Math.PI * 0.22;
      const radius = 24.5 - t * 2.0;
      const height = 8.0 + Math.sin(t * Math.PI) * 3.6;

      const camX = this.swordStadiumCenter.x + Math.sin(angle) * radius;
      const camZ = this.swordStadiumCenter.z + Math.cos(angle) * radius;

      this.targetCamPos.set(camX, height, camZ);
      this.targetLookAt.copy(this.swordStadiumCenter);
    }
    // 7. 전환 3 & 피날레: 3대 경기장 전체 조망 항공뷰로 상승 (Step 10, p: 0.92 ~ 1.00)
    else {
      const rawT = (this.scrollProgress - 0.92) / 0.08;
      const s = rawT * rawT * (3 - 2 * rawT);

      const p0 = new THREE.Vector3(142, 18, 160);
      const p1 = new THREE.Vector3(80, 135, 330);
      const p2 = this.skyCamPos;

      const oneMinusS = 1 - s;
      const posX = oneMinusS * oneMinusS * p0.x + 2 * oneMinusS * s * p1.x + s * s * p2.x;
      const posY = oneMinusS * oneMinusS * p0.y + 2 * oneMinusS * s * p1.y + s * s * p2.y;
      const posZ = oneMinusS * oneMinusS * p0.z + 2 * oneMinusS * s * p1.z + s * s * p2.z;
      this.targetCamPos.set(posX, posY, posZ);

      const look0 = this.swordStadiumCenter;
      const look1 = new THREE.Vector3(-10, 18, 50);
      const look2 = this.skyLookAt;
      const lookX = oneMinusS * oneMinusS * look0.x + 2 * oneMinusS * s * look1.x + s * s * look2.x;
      const lookY = oneMinusS * oneMinusS * look0.y + 2 * oneMinusS * s * look1.y + s * s * look2.y;
      const lookZ = oneMinusS * oneMinusS * look0.z + 2 * oneMinusS * s * look1.z + s * s * look2.z;
      this.targetLookAt.set(lookX, lookY, lookZ);
    }
  }

  initEventListeners() {
    window.addEventListener('resize', () => {
      if (!this.renderer || !this.camera) return;
      const w = window.innerWidth;
      const h = window.innerHeight;
      this.camera.aspect = w / h;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(w, h);
    });
  }

  animate() {
    requestAnimationFrame(this.animate);
    const delta = this.clock.getDelta();
    const elapsed = this.clock.getElapsedTime();

    // 1. 솜사탕 구름 부드러운 자전
    if (this.cloudsGroup) {
      this.cloudsGroup.rotation.y += delta * 0.007;
    }

    // 2. 카툰 에메랄드 수면 파도 웨이브 & 텍스처 흐름 애니메이션
    if (this.waterMesh) {
      if (this.waterMesh.material && this.waterMesh.material.map) {
        this.waterMesh.material.map.offset.x = (this.waterMesh.material.map.offset.x + delta * 0.012) % 1;
        this.waterMesh.material.map.offset.y = (this.waterMesh.material.map.offset.y + delta * 0.008) % 1;
      }
      this.waterMesh.material.opacity = 0.93 + Math.sin(elapsed * 1.5) * 0.02;
    }

    // 3. 리조트 세일보트 잔물결 롤링/피칭
    if (this.boatsGroup) {
      this.boatsGroup.children.forEach((boat, i) => {
        boat.position.y = -0.2 + Math.sin(elapsed * 1.8 + i * 2) * 0.25;
        boat.rotation.z = Math.sin(elapsed * 1.2 + i * 1.5) * 0.04;
      });
    }

    // 4. 알록달록 하늘 풍선 유유히 둥둥 부유
    if (this.balloonsSystem) {
      this.balloonsSystem.update(elapsed);
    }

    // 5. 밤낮 가리지 않고 하늘에 계속 펑펑 터지는 축제 폭죽
    if (this.fireworksSystem) {
      this.fireworksSystem.update(delta);
    }

    // 경기장 간 이동 시 어지럽지 않도록 여유롭고 부드러운 시네마틱 댐핑 (0.042)
    const lerpFactor = this.scrollProgress <= 0.04 ? 0.065 : 0.042;
    this.currentCamPos.lerp(this.targetCamPos, lerpFactor);
    this.currentLookAt.lerp(this.targetLookAt, lerpFactor);

    this.camera.position.copy(this.currentCamPos);
    this.camera.lookAt(this.currentLookAt);

    this.renderer.render(this.scene, this.camera);
  }
}

export { TennisIslandCinematicViewer, ModelInspectModalViewer };
