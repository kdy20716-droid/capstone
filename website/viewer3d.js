import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { FBXLoader } from 'three/addons/loaders/FBXLoader.js';

/* =========================================================================
   1. 그랜드 챔피언십 테니스 코트 생성기 (오스트레일리안 오픈 블루 & 선명한 규격 라인)
========================================================================= */
function createTennisCourtMesh() {
  const canvas = document.createElement('canvas');
  canvas.width = 1024;
  canvas.height = 2048;
  const ctx = canvas.getContext('2d');

  // 1. 코트 외곽 런오프/에이프런 (지중해 리조트 네이비 블루)
  ctx.fillStyle = '#0284c7';
  ctx.fillRect(0, 0, 1024, 2048);

  // 2. 인필드 플레이 코트 영역 (오스트레일리안 오픈 일렉트릭 시안 블루)
  const courtX = 140;
  const courtY = 240;
  const courtW = 744;
  const courtH = 1568;

  ctx.fillStyle = '#38bdf8';
  ctx.fillRect(courtX, courtY, courtW, courtH);

  // 3. 고선명 규격 라인 (순백색)
  ctx.strokeStyle = '#ffffff';
  ctx.lineWidth = 14;

  // 복식 외곽 라인
  ctx.strokeRect(courtX, courtY, courtW, courtH);

  // 단식 사이드 라인 (양쪽 인셋)
  const singlesInset = Math.round(courtW * 0.125);
  ctx.beginPath();
  ctx.moveTo(courtX + singlesInset, courtY);
  ctx.lineTo(courtX + singlesInset, courtY + courtH);
  ctx.moveTo(courtX + courtW - singlesInset, courtY);
  ctx.lineTo(courtX + courtW - singlesInset, courtY + courtH);
  ctx.stroke();

  // 중앙 네트 라인
  const centerY = courtY + courtH / 2;
  ctx.lineWidth = 10;
  ctx.beginPath();
  ctx.moveTo(courtX - 50, centerY);
  ctx.lineTo(courtX + courtW + 50, centerY);
  ctx.stroke();

  // 서비스 라인 (네트로부터 53.8% 거리)
  const serviceDist = Math.round((courtH / 2) * (21 / 39));
  const serviceYTop = centerY - serviceDist;
  const serviceYBottom = centerY + serviceDist;

  ctx.lineWidth = 14;
  ctx.beginPath();
  // 상단 서비스 라인
  ctx.moveTo(courtX + singlesInset, serviceYTop);
  ctx.lineTo(courtX + courtW - singlesInset, serviceYTop);
  // 하단 서비스 라인
  ctx.moveTo(courtX + singlesInset, serviceYBottom);
  ctx.lineTo(courtX + courtW - singlesInset, serviceYBottom);
  // 센터 서비스 라인
  ctx.moveTo(courtX + courtW / 2, serviceYTop);
  ctx.lineTo(courtX + courtW / 2, serviceYBottom);
  ctx.stroke();

  // 베이스라인 중앙 센터 마크
  const tickLen = 45;
  ctx.beginPath();
  ctx.moveTo(courtX + courtW / 2, courtY);
  ctx.lineTo(courtX + courtW / 2, courtY + tickLen);
  ctx.moveTo(courtX + courtW / 2, courtY + courtH);
  ctx.lineTo(courtX + courtW / 2, courtY + courtH - tickLen);
  ctx.stroke();

  const texture = new THREE.CanvasTexture(canvas);
  texture.anisotropy = 8;

  // X축 폭 30, Z축 길이 52 (선명하고 웅장한 챔피언십 블루 & 시안 코트)
  const courtGeo = new THREE.PlaneGeometry(30, 52);
  const courtMat = new THREE.MeshStandardMaterial({
    map: texture,
    roughness: 0.45,
    metalness: 0.05,
    side: THREE.DoubleSide,
    polygonOffset: true,
    polygonOffsetFactor: -6,
    polygonOffsetUnits: -6
  });
  const courtMesh = new THREE.Mesh(courtGeo, courtMat);
  courtMesh.rotation.x = -Math.PI / 2;
  courtMesh.position.y = 0.28; // 스타디움 하부 바닥보다 위에 확실히 배치
  courtMesh.receiveShadow = true;
  return courtMesh;
}

/* =========================================================================
   2. ACE DOME: 거대 돔 형식 외벽 껍데기 모델링
   (기존 관중석 외벽 깨짐/사각 모서리 완전 커버 & 웅장한 스포츠 돔 건축)
========================================================================= */
function addGrandAceDomeShell(stadiumUnit) {
  const domeGroup = new THREE.Group();
  domeGroup.name = 'grand_ace_dome_shell';

  const segments = 64;

  // ── 경기장 외부 돔 (사이즈 살짝 최적화 & 깔끔한 원통형 림) ──
  // ── 경기장 외부 돔 (사이즈 최적화 & 내부 코트 시원하게 개방) ──
  // 1. 하단 광장 포디엄 외벽 튜브 (안쪽이 뻥 뚫린 링 형태: 경기장 내부 하얀 바닥 차단)
  const plazaGeo = new THREE.CylinderGeometry(73, 76, 1.8, segments, 1, true);
  const plazaMat = new THREE.MeshStandardMaterial({
    color: 0xe2e8f0,
    roughness: 0.85,
    metalness: 0.05,
    side: THREE.DoubleSide
  });
  const plazaMesh = new THREE.Mesh(plazaGeo, plazaMat);
  plazaMesh.position.y = 0.9;
  plazaMesh.receiveShadow = true;
  domeGroup.add(plazaMesh);

  // 외벽 바깥쪽 상단 링 에이프런 (반지름 67 ~ 76: 내부 코트 영역은 100% 개방)
  const apronGeo = new THREE.RingGeometry(67, 76, segments);
  const apronMesh = new THREE.Mesh(apronGeo, plazaMat);
  apronMesh.rotation.x = -Math.PI / 2;
  apronMesh.position.y = 1.8;
  apronMesh.receiveShadow = true;
  domeGroup.add(apronMesh);

  // 2. 돔 하부 유선형 외벽 (반지름 67 ~ 72, 높이 16)
  const lowerWallGeo = new THREE.CylinderGeometry(67, 72, 16, segments, 1, true);
  const whitePanelMat = new THREE.MeshStandardMaterial({
    color: 0xffffff,
    roughness: 0.55,
    metalness: 0.1,
    side: THREE.DoubleSide
  });
  const lowerWall = new THREE.Mesh(lowerWallGeo, whitePanelMat);
  lowerWall.position.y = 9.8;
  lowerWall.castShadow = true;
  lowerWall.receiveShadow = true;
  domeGroup.add(lowerWall);

  // 3. 시그니처 블루 리본 밴드 (반지름 67.8~68.2, 높이 4.4)
  const ribbonGeo = new THREE.CylinderGeometry(67.8, 68.2, 4.4, segments, 1, true);
  const blueRibbonMat = new THREE.MeshStandardMaterial({
    color: 0x0077e6,
    roughness: 0.35,
    metalness: 0.25,
    side: THREE.DoubleSide
  });
  const ribbon = new THREE.Mesh(ribbonGeo, blueRibbonMat);
  ribbon.position.y = 16.5;
  domeGroup.add(ribbon);

  // 4. 발광 시안 LED 엣지 링
  const ledGeo = new THREE.TorusGeometry(68.0, 0.35, 8, segments);
  const ledMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8 });
  const ledTop = new THREE.Mesh(ledGeo, ledMat);
  ledTop.rotation.x = Math.PI / 2;
  ledTop.position.y = 18.8;
  domeGroup.add(ledTop);

  const ledBot = new THREE.Mesh(ledGeo, ledMat);
  ledBot.rotation.x = Math.PI / 2;
  ledBot.position.y = 14.2;
  domeGroup.add(ledBot);

  // 5. 돔 상부 완만한 유선형 지붕 (원형 오큘러스 개구부로 연결, y: 18.8 ~ 34.5)
  // 티어 1: 반지름 67.8 -> 60, 높이 6.2
  const domeTier1Geo = new THREE.CylinderGeometry(60, 67.8, 6.2, segments, 1, true);
  const domeTier1 = new THREE.Mesh(domeTier1Geo, whitePanelMat);
  domeTier1.position.y = 21.9;
  domeTier1.castShadow = true;
  domeTier1.receiveShadow = true;
  domeGroup.add(domeTier1);

  // 티어 2: 반지름 60 -> 50, 높이 6.2
  const domeTier2Geo = new THREE.CylinderGeometry(50, 60, 6.2, segments, 1, true);
  const domeTier2 = new THREE.Mesh(domeTier2Geo, whitePanelMat);
  domeTier2.position.y = 28.1;
  domeTier2.castShadow = true;
  domeTier2.receiveShadow = true;
  domeGroup.add(domeTier2);

  // 티어 3: 반지름 50 -> 43, 높이 3.5 (상단 거대 원형 구멍 림)
  const domeTier3Geo = new THREE.CylinderGeometry(43, 50, 3.5, segments, 1, true);
  const domeTier3 = new THREE.Mesh(domeTier3Geo, whitePanelMat);
  domeTier3.position.y = 33.0;
  domeTier3.castShadow = true;
  domeTier3.receiveShadow = true;
  domeGroup.add(domeTier3);

  // 6. 센터코트 스카이라이트 거대 원형 오큘러스 림 (천장 동그라미 구멍 테두리)
  const oculusGeo = new THREE.RingGeometry(41, 43.8, segments);
  const oculusMat = new THREE.MeshStandardMaterial({
    color: 0xffffff,
    roughness: 0.25,
    metalness: 0.35,
    side: THREE.DoubleSide
  });
  const oculus = new THREE.Mesh(oculusGeo, oculusMat);
  oculus.rotation.x = -Math.PI / 2;
  oculus.position.y = 34.8;
  domeGroup.add(oculus);

  // 오큘러스 글래스 링 (반경 39 ~ 41)
  const glassGeo = new THREE.RingGeometry(39, 41, segments);
  const glassMat = new THREE.MeshStandardMaterial({
    color: 0x38bdf8,
    transparent: true,
    opacity: 0.5,
    roughness: 0.15,
    metalness: 0.4,
    side: THREE.DoubleSide
  });
  const glass = new THREE.Mesh(glassGeo, glassMat);
  glass.rotation.x = -Math.PI / 2;
  glass.position.y = 34.78;
  domeGroup.add(glass);

  // 7. 돔 외곽 원기둥 수직 건축 기둥 루버 (36개 입체 핀으로 원통형 경기장 파사드 완성)
  const colGeo = new THREE.BoxGeometry(1.1, 18.5, 2.5);
  const colMat = new THREE.MeshStandardMaterial({
    color: 0x0284c7,
    roughness: 0.45,
    metalness: 0.3
  });

  for (let i = 0; i < 36; i++) {
    const angle = (i / 36) * Math.PI * 2;
    const col = new THREE.Mesh(colGeo, colMat);
    const rad = 70.8;
    col.position.set(Math.cos(angle) * rad, 10.0, Math.sin(angle) * rad);
    col.rotation.y = -angle + Math.PI / 2;
    col.castShadow = true;
    domeGroup.add(col);
  }

  // 8. 4대 메인 출입구 파빌리온 (동/서/남/북)
  const gateGeo = new THREE.BoxGeometry(6.0, 8.0, 4.0);
  const gateMat = new THREE.MeshStandardMaterial({ color: 0x0077e6, roughness: 0.4 });
  const gateGlassMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8, transparent: true, opacity: 0.8 });

  for (let i = 0; i < 4; i++) {
    const angle = (i / 4) * Math.PI * 2;
    const gate = new THREE.Group();
    const frame = new THREE.Mesh(gateGeo, gateMat);
    frame.position.y = 4.0;
    const innerGlass = new THREE.Mesh(new THREE.PlaneGeometry(4.8, 6.6), gateGlassMat);
    innerGlass.position.set(0, 3.8, 2.01);

    gate.add(frame);
    gate.add(innerGlass);
    const grad = 71.8;
    gate.position.set(Math.cos(angle) * grad, 0, Math.sin(angle) * grad);
    gate.rotation.y = -angle + Math.PI / 2;
    domeGroup.add(gate);
  }

  stadiumUnit.add(domeGroup);
}

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

    // 5x Island Scale:
    // Left Main Stadium center at x: -160, z: 0
    this.leftStadiumCenter = new THREE.Vector3(-160, 1.2, 0);

    // Sky Camera Position (Higher and wider to frame the entire 5x mega island)
    this.skyCamPos = new THREE.Vector3(0, 340, 420);
    this.skyLookAt = new THREE.Vector3(0, 0, 0);

    this.currentCamPos = new THREE.Vector3().copy(this.skyCamPos);
    this.targetCamPos = new THREE.Vector3().copy(this.skyCamPos);
    this.currentLookAt = new THREE.Vector3().copy(this.skyLookAt);
    this.targetLookAt = new THREE.Vector3().copy(this.skyLookAt);

    this.scrollProgress = 0;
    this.particles = null;

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
    this.renderer.toneMappingExposure = 1.2;

    this.container.innerHTML = '';
    this.container.appendChild(this.renderer.domElement);
  }

  /* 화사하고 감성적인 파스텔 블루 & 웜 선샤인 스카이 */
  initEmotionalSky() {
    const canvas = document.createElement('canvas');
    canvas.width = 512;
    canvas.height = 512;
    const ctx = canvas.getContext('2d');

    const grad = ctx.createLinearGradient(0, 0, 0, 512);
    grad.addColorStop(0, '#0284c7');    // Rich Mediterranean Blue
    grad.addColorStop(0.35, '#38bdf8'); // Cheerful Tropical Cyan
    grad.addColorStop(0.7, '#bae6fd');  // Soft Pastel Sky
    grad.addColorStop(0.92, '#fef08a'); // Warm Golden Sunset Tint
    grad.addColorStop(1, '#ffffff');    // Bright Horizon

    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, 512, 512);

    this.scene.background = new THREE.CanvasTexture(canvas);

    // Fluffy Ambient 3D Clouds
    this.cloudsGroup = new THREE.Group();
    const cloudGeo = new THREE.DodecahedronGeometry(18, 1);
    const cloudMat = new THREE.MeshBasicMaterial({ color: 0xffffff, transparent: true, opacity: 0.88 });

    for (let i = 0; i < 28; i++) {
      const cloud = new THREE.Mesh(cloudGeo, cloudMat);
      const angle = (i / 28) * Math.PI * 2;
      const radius = 550 + Math.random() * 250;
      cloud.position.set(
        Math.cos(angle) * radius,
        120 + Math.random() * 80,
        Math.sin(angle) * radius
      );
      cloud.scale.set(2.5 + Math.random() * 2.5, 0.7 + Math.random() * 0.5, 1.8 + Math.random() * 1.5);
      this.cloudsGroup.add(cloud);
    }
    this.scene.add(this.cloudsGroup);
  }

  /* 유기적 형태의 평평한 리조트 메가 섬 & 산호초 & 부속 섬 3개 */
  initVastOceanAndMegaIsland() {
    // 1. 끝없는 푸른 지중해/열대 바다
    const oceanGeo = new THREE.PlaneGeometry(10000, 10000, 60, 60);
    const oceanMat = new THREE.MeshStandardMaterial({
      color: 0x0284c7,
      roughness: 0.15,
      metalness: 0.25,
      transparent: true,
      opacity: 0.94
    });
    this.waterMesh = new THREE.Mesh(oceanGeo, oceanMat);
    this.waterMesh.rotation.x = -Math.PI / 2;
    this.waterMesh.position.y = -0.8;
    this.scene.add(this.waterMesh);

    // 유기적인 곡선 섬 외곽 형태를 생성하는 헬퍼 (평평함 유지)
    const createOrganicShape = (baseRadius, noiseScale = 0.12, pointsCount = 120, seed = 0) => {
      const shape = new THREE.Shape();
      for (let i = 0; i <= pointsCount; i++) {
        const theta = (i / pointsCount) * Math.PI * 2;
        // 자연스러운 닌텐도 스포츠 리조트 해안선 웨이브
        const wave = 
          Math.sin(theta * 3 + seed) * (baseRadius * noiseScale) +
          Math.cos(theta * 5 - seed * 1.5) * (baseRadius * noiseScale * 0.5) +
          Math.sin(theta * 2 + 1.2) * (baseRadius * 0.08);
        const r = baseRadius + wave;
        const x = Math.cos(theta) * r;
        const y = Math.sin(theta) * r;
        if (i === 0) shape.moveTo(x, y);
        else shape.lineTo(x, y);
      }
      return shape;
    };

    // 2. 에메랄드 산호초 얕은 바다 쉘프 (Coral Reef Shelf)
    const coralShape = createOrganicShape(480, 0.13, 120, 1.2);
    const coralGeo = new THREE.ShapeGeometry(coralShape);
    const coralMat = new THREE.MeshBasicMaterial({
      color: 0x38bdf8,
      transparent: true,
      opacity: 0.5,
      side: THREE.DoubleSide
    });
    const coralMesh = new THREE.Mesh(coralGeo, coralMat);
    coralMesh.rotation.x = -Math.PI / 2;
    coralMesh.position.y = -0.6;
    this.islandGroup.add(coralMesh);

    // 3. 백사장 / 골든 샌드 비치 (유기적 해안선)
    const sandShape = createOrganicShape(435, 0.11, 120, 0.5);
    const sandGeo = new THREE.ShapeGeometry(sandShape);
    const sandMat = new THREE.MeshStandardMaterial({
      color: 0xfde047, // 따스한 열대 모래사장
      roughness: 0.9,
      metalness: 0.02,
      side: THREE.DoubleSide
    });
    const sandMesh = new THREE.Mesh(sandGeo, sandMat);
    sandMesh.rotation.x = -Math.PI / 2;
    sandMesh.position.y = -0.3;
    this.islandGroup.add(sandMesh);

    // 4. 메인 평평한 잔디 섬 (Lush Green Flat Resort Plateau)
    const grassShape = createOrganicShape(395, 0.09, 120, 0.2);
    const grassGeo = new THREE.ShapeGeometry(grassShape);
    const grassMat = new THREE.MeshStandardMaterial({
      color: 0x4ade80, // 화사하고 생생한 닌텐도 스타일 잔디
      roughness: 0.7,
      metalness: 0.04,
      side: THREE.DoubleSide
    });
    const grassMesh = new THREE.Mesh(grassGeo, grassMat);
    grassMesh.rotation.x = -Math.PI / 2;
    grassMesh.position.y = -0.05;
    grassMesh.receiveShadow = true;
    this.islandGroup.add(grassMesh);

    // 5. 주변의 매력적인 평평한 부속 섬들 (리조트 아일랜드 군도 분위기)
    const subIslands = [
      { x: -380, z: -280, r: 75, color: 0x4ade80 },
      { x: 360, z: -320, r: 85, color: 0x4ade80 },
      { x: 380, z: 290, r: 70, color: 0x4ade80 }
    ];

    subIslands.forEach((isle, idx) => {
      // 부속 섬 산호초
      const subCoral = new THREE.Mesh(
        new THREE.ShapeGeometry(createOrganicShape(isle.r * 1.35, 0.15, 60, idx + 2)),
        coralMat
      );
      subCoral.rotation.x = -Math.PI / 2;
      subCoral.position.set(isle.x, -0.6, isle.z);
      this.islandGroup.add(subCoral);

      // 부속 섬 모래사장
      const subSand = new THREE.Mesh(
        new THREE.ShapeGeometry(createOrganicShape(isle.r * 1.15, 0.12, 60, idx + 4)),
        sandMat
      );
      subSand.rotation.x = -Math.PI / 2;
      subSand.position.set(isle.x, -0.3, isle.z);
      this.islandGroup.add(subSand);

      // 부속 섬 잔디
      const subGrass = new THREE.Mesh(
        new THREE.ShapeGeometry(createOrganicShape(isle.r, 0.1, 60, idx + 6)),
        grassMat
      );
      subGrass.rotation.x = -Math.PI / 2;
      subGrass.position.set(isle.x, -0.05, isle.z);
      subGrass.receiveShadow = true;
      this.islandGroup.add(subGrass);
    });

    // 6. 열대 야자수 및 리조트 수목 배치 (1.5배 돔 스타디움과 여유로운 간격 확보)
    const trunkGeo = new THREE.CylinderGeometry(0.8, 1.3, 8, 8);
    const trunkMat = new THREE.MeshStandardMaterial({ color: 0x854d0e, roughness: 0.8 });
    const leavesGeo = new THREE.SphereGeometry(4.5, 8, 8);
    const leavesMat = new THREE.MeshStandardMaterial({ color: 0x16a34a, roughness: 0.6 });

    for (let i = 0; i < 110; i++) {
      const angle = Math.random() * Math.PI * 2;
      const r = 210 + Math.random() * 165;
      const tx = Math.cos(angle) * r;
      const tz = Math.sin(angle) * r;

      // 1.5배 확대된 스타디움 돔(반지름 ~84)과의 충돌 완전 방지 (클리어런스 98)
      const distToLeft = Math.hypot(tx - (-160), tz);
      const distToTopRight = Math.hypot(tx - 130, tz - (-150));
      const distToBottomRight = Math.hypot(tx - 130, tz - 150);

      if (distToLeft < 98 || distToTopRight < 95 || distToBottomRight < 95) continue;

      const tree = new THREE.Group();
      const trunk = new THREE.Mesh(trunkGeo, trunkMat);
      trunk.position.y = 4;
      trunk.rotation.z = (Math.random() - 0.5) * 0.15;
      const leaves = new THREE.Mesh(leavesGeo, leavesMat);
      leaves.position.y = 9.5;
      leaves.scale.set(1 + Math.random() * 0.4, 0.7, 1 + Math.random() * 0.4);

      tree.add(trunk);
      tree.add(leaves);
      tree.position.set(tx, 0, tz);
      this.islandGroup.add(tree);
    }
  }

  initAtmosphereParticles() {
    // 하얀색 네모 떨어지는 파티클 완전 비활성화
  }

  initLights() {
    const ambientLight = new THREE.AmbientLight(0xffffff, 1.5);
    this.scene.add(ambientLight);

    const sunLight = new THREE.DirectionalLight(0xfffae0, 2.5);
    sunLight.position.set(200, 350, 250);
    sunLight.castShadow = true;
    sunLight.shadow.mapSize.width = 2048;
    sunLight.shadow.mapSize.height = 2048;
    sunLight.shadow.camera.near = 50;
    sunLight.shadow.camera.far = 800;
    const d = 300;
    sunLight.shadow.camera.left = -d;
    sunLight.shadow.camera.right = d;
    sunLight.shadow.camera.top = d;
    sunLight.shadow.camera.bottom = -d;
    this.scene.add(sunLight);

    const fillLight = new THREE.DirectionalLight(0x7dd3fc, 0.9);
    fillLight.position.set(-200, 150, -200);
    this.scene.add(fillLight);
  }

  /* 5배 넓어진 섬 위에 3대 경기장 여유로운 삼각 배치 */
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
            child.castShadow = true;
            const name = (child.name || '').toLowerCase();
            if (name.includes('chair_main')) {
              child.material = new THREE.MeshStandardMaterial({ map: chairMainTex, roughness: 0.5, side: THREE.DoubleSide });
            } else if (name.includes('chair_rear')) {
              child.material = new THREE.MeshStandardMaterial({ map: chairRearTex, roughness: 0.5, side: THREE.DoubleSide });
            } else if (name.includes('extra')) {
              child.material = new THREE.MeshStandardMaterial({ map: extraTex, roughness: 0.5, side: THREE.DoubleSide });
            } else {
              child.material = new THREE.MeshStandardMaterial({ map: courtTex, roughness: 0.65, side: THREE.DoubleSide });
            }
          }
        });
        const box = new THREE.Box3().setFromObject(courtFbx);
        const center = box.getCenter(new THREE.Vector3());
        courtFbx.position.x -= center.x;
        courtFbx.position.y -= box.min.y;
        courtFbx.position.z -= center.z;
        stadiumUnit.add(courtFbx);

        fbxLoader.load('assets/models/court/Tennis_Back.fbx', (standsFbx) => {
          standsFbx.traverse((child) => {
            if (child.isMesh) {
              child.castShadow = true;
              child.receiveShadow = true;
              const name = (child.name || '').toLowerCase();
              if (name.includes('light')) {
                child.material = new THREE.MeshStandardMaterial({
                  map: lightTex,
                  emissive: new THREE.Color(0xfff4cc),
                  emissiveIntensity: 0.4,
                  side: THREE.DoubleSide
                });
              } else if (name.includes('back_2') || name.includes('back2')) {
                child.material = new THREE.MeshStandardMaterial({ map: back2Tex, roughness: 0.6, side: THREE.DoubleSide });
              } else if (name.includes('op')) {
                child.material = new THREE.MeshStandardMaterial({ map: opTex, transparent: true, alphaTest: 0.2, side: THREE.DoubleSide });
              } else {
                child.material = new THREE.MeshStandardMaterial({ map: back1Tex, roughness: 0.6, side: THREE.DoubleSide });
              }
            }
          });
          const b = new THREE.Box3().setFromObject(standsFbx);
          const c = b.getCenter(new THREE.Vector3());
          standsFbx.position.x -= c.x;
          standsFbx.position.y -= b.min.y;
          standsFbx.position.z -= c.z;
          stadiumUnit.add(standsFbx);

          // -----------------------------------------------------------------
          // 1. 그랜드 챔피언십 테니스 코트 바닥 (오스트레일리안 오픈 블루 & 선명한 규격 라인)
          // -----------------------------------------------------------------
          const courtFloor = createTennisCourtMesh();
          stadiumUnit.add(courtFloor);

          // -----------------------------------------------------------------
          // 2. ACE DOME: 거대 돔형 외벽 껍데기 래퍼 (외부 깨짐 완벽 커버 & 웅장한 스포츠 돔)
          // -----------------------------------------------------------------
          addGrandAceDomeShell(stadiumUnit);

          onReady(stadiumUnit);
        });
      });
    };

    // 1. Left Stadium (Main focus stadium at x: -160, z: 0)
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

    // 2. Top-Right Stadium (Wide spacing at x: 130, z: -150)
    buildStadiumUnit((unit) => {
      this.topRightStadiumGroup.add(unit);
      this.topRightStadiumGroup.position.set(130, 0, -150);
      this.topRightStadiumGroup.rotation.y = -Math.PI / 4;
      this.islandGroup.add(this.topRightStadiumGroup);
    });

    // 3. Bottom-Right Stadium (Wide spacing at x: 130, z: 150)
    buildStadiumUnit((unit) => {
      this.bottomRightStadiumGroup.add(unit);
      this.bottomRightStadiumGroup.position.set(130, 0, 150);
      this.bottomRightStadiumGroup.rotation.y = Math.PI / 4;
      this.islandGroup.add(this.bottomRightStadiumGroup);
    });
  }

  /* 스크롤 카메라 궤적:
     - p = 0.0: 높은 하늘에서 5배 확장된 거대 테니스 섬과 3개 경기장 전경 조망
     - p > 0.06: 왼쪽 메인 경기장(-160, 1.2, 0) 안으로 빠르게 "슈웅" 강하하여 코트 주변을 시네마틱 회전
  */
  updateCameraForScroll(p) {
    this.scrollProgress = Math.max(0, Math.min(1, p));

    if (this.scrollProgress <= 0.06) {
      const factor = this.scrollProgress / 0.06;
      this.targetCamPos.lerpVectors(this.skyCamPos, new THREE.Vector3(-100, 160, 180), factor);
      this.targetLookAt.lerpVectors(this.skyLookAt, this.leftStadiumCenter, factor);
    } else {
      const stadiumProgress = (this.scrollProgress - 0.06) / 0.94;

      const angle = stadiumProgress * Math.PI * 1.6;
      const radius = 24 - stadiumProgress * 4;
      const height = 7.5 + Math.sin(stadiumProgress * Math.PI) * 5.0;

      const camX = this.leftStadiumCenter.x + Math.sin(angle) * radius;
      const camZ = this.leftStadiumCenter.z + Math.cos(angle) * radius;
      const camY = height;

      this.targetCamPos.set(camX, camY, camZ);
      this.targetLookAt.copy(this.leftStadiumCenter);
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

    // Clouds and water
    if (this.cloudsGroup) {
      this.cloudsGroup.rotation.y += delta * 0.008;
    }

    if (this.waterMesh) {
      this.waterMesh.material.opacity = 0.92 + Math.sin(this.clock.getElapsedTime() * 1.8) * 0.03;
    }

    // Fast silky smooth camera dive "슈웅"
    const lerpFactor = this.scrollProgress <= 0.12 ? 0.09 : 0.06;
    this.currentCamPos.lerp(this.targetCamPos, lerpFactor);
    this.currentLookAt.lerp(this.targetLookAt, lerpFactor);

    this.camera.position.copy(this.currentCamPos);
    this.camera.lookAt(this.currentLookAt);

    this.renderer.render(this.scene, this.camera);
  }
}

/* =========================================================================
   3D Model Showcase Inspector (마우스 드래그 뷰어 모달)
========================================================================= */
class ModelInspectModalViewer {
  constructor(canvasContainerId) {
    this.container = document.getElementById(canvasContainerId);
    if (!this.container) return;

    this.clock = new THREE.Clock();
    this.textureLoader = new THREE.TextureLoader();
    this.currentObject = null;
    this.mixer = null;
    this.currentModelType = 'character';
    this.racketMesh = null;
    this.ballMesh = null;
    this.isBouncing = true;
    this.ballTime = 0;

    this.initScene();
    this.initLights();
    this.initControls();
    this.loadModel('character');

    this.animate = this.animate.bind(this);
    requestAnimationFrame(this.animate);
  }

  initScene() {
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0xf0f9ff);

    const w = this.container.clientWidth || 600;
    const h = this.container.clientHeight || 450;

    this.camera = new THREE.PerspectiveCamera(45, w / h, 0.1, 500);
    this.camera.position.set(0, 1.5, 4);

    this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: false });
    this.renderer.setSize(w, h);
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    this.renderer.shadowMap.enabled = true;

    this.container.innerHTML = '';
    this.container.appendChild(this.renderer.domElement);

    const grid = new THREE.GridHelper(16, 16, 0x0284c7, 0xbae6fd);
    grid.position.y = -0.01;
    this.scene.add(grid);
  }

  initLights() {
    const amb = new THREE.AmbientLight(0xffffff, 1.4);
    this.scene.add(amb);

    const dir = new THREE.DirectionalLight(0xffffff, 1.8);
    dir.position.set(5, 10, 8);
    dir.castShadow = true;
    this.scene.add(dir);

    const fill = new THREE.DirectionalLight(0x7dd3fc, 0.8);
    fill.position.set(-5, 5, -5);
    this.scene.add(fill);
  }

  initControls() {
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.08;
    this.controls.maxPolarAngle = Math.PI / 2 - 0.01;
  }

  fitCamera(obj, offset = 1.3) {
    const box = new THREE.Box3().setFromObject(obj);
    const size = box.getSize(new THREE.Vector3());
    const center = box.getCenter(new THREE.Vector3());

    let maxDim = Math.max(size.x, size.y, size.z);
    if (!isFinite(maxDim) || maxDim <= 0.001) maxDim = 2;

    obj.position.x -= center.x;
    obj.position.y -= box.min.y;
    obj.position.z -= center.z;

    const fitDist = offset * (maxDim / (2 * Math.atan((Math.PI * this.camera.fov) / 360)));
    const dir = new THREE.Vector3(1, 0.6, 1.2).normalize();
    this.camera.position.copy(dir.multiplyScalar(fitDist));
    this.camera.position.y += (size.y || 1) * 0.4;
    this.controls.target.set(0, (size.y || 1) * 0.45, 0);
    this.controls.minDistance = Math.max(0.2, maxDim * 0.2);
    this.controls.maxDistance = Math.max(15, maxDim * 6);
    this.controls.update();
  }

  clearModel() {
    if (this.currentObject) {
      this.scene.remove(this.currentObject);
      this.currentObject = null;
    }
    if (this.mixer) {
      this.mixer.stopAllAction();
      this.mixer = null;
    }
    this.racketMesh = null;
    this.ballMesh = null;
  }

  loadModel(type) {
    this.currentModelType = type;
    this.clearModel();

    const gltfLoader = new GLTFLoader();
    const fbxLoader = new FBXLoader();

    if (type === 'character') {
      this.loadCharacterMotion('reap_swing');
    } else if (type === 'racket') {
      this.racketMeshes = [];
      fbxLoader.load('assets/models/racket/Tennis_Racket.fbx', (racket) => {
        this.currentObject = racket;
        const frameTex = this.textureLoader.load('assets/models/racket/Texture/racket_blue.png');
        const strTex = this.textureLoader.load('assets/models/racket/Texture/Tennis_Racket_Tennis_Racket_Strings_AlbedoTransparency.png');

        racket.traverse((child) => {
          if (child.isMesh) {
            child.castShadow = true;
            const name = (child.name || '').toLowerCase();
            if (name.includes('string')) {
              child.material = new THREE.MeshStandardMaterial({
                map: strTex,
                transparent: true,
                alphaTest: 0.3,
                side: THREE.DoubleSide
              });
            } else {
              this.racketMeshes.push(child);
              child.material = new THREE.MeshStandardMaterial({
                map: frameTex,
                roughness: 0.35,
                metalness: 0.2,
                side: THREE.DoubleSide
              });
            }
          }
        });
        this.scene.add(racket);
        this.fitCamera(racket, 1.4);
      });
    } else if (type === 'ball') {
      this.isBouncing = false; // 테니스 공을 통통 튀지 않고 가만히 멈춰서 관찰하도록 설정
      fbxLoader.load('assets/models/ball/tennis_ball.fbx', (ball) => {
        this.currentObject = ball;
        this.ballMesh = ball;

        const ballTex = this.textureLoader.load('assets/models/ball/Texture/tennis_ball_tennis_ball1_AlbedoTransparency.png');
        const ballNorm = this.textureLoader.load('assets/models/ball/Texture/tennis_ball_tennis_ball1_Normal.png');

        ball.traverse((child) => {
          if (child.isMesh) {
            child.castShadow = true;
            child.material = new THREE.MeshStandardMaterial({
              map: ballTex,
              normalMap: ballNorm,
              roughness: 0.85,
              metalness: 0.05
            });
          }
        });
        this.scene.add(ball);
        this.fitCamera(ball, 1.4);
      });
    } else if (type === 'court') {
      fbxLoader.load('assets/models/court/Tennis_.fbx', (court) => {
        this.currentObject = court;
        court.rotation.y = Math.PI / 2;
        const courtTex = this.textureLoader.load('assets/models/court/Texture/Tennis__Tennis_AlbedoTransparency.png');
        court.traverse((c) => {
          if (c.isMesh) {
            c.material = new THREE.MeshStandardMaterial({ map: courtTex, roughness: 0.65, side: THREE.DoubleSide });
          }
        });
        const floorMesh = createTennisCourtMesh();
        court.add(floorMesh);
        this.scene.add(court);
        this.fitCamera(court, 1.2);
      });
    }
  }

  /* 캐릭터 5가지 역동적인 모션 교체 로더 */
  loadCharacterMotion(motionKey = 'reap_swing') {
    this.clearModel();
    this.currentModelType = 'character';

    const motionMap = {
      reap_swing: { path: 'assets/models/character/character.glb', type: 'gltf' },
      spin_attack: { path: 'assets/models/character/spin_attack.glb', type: 'gltf' },
      slash_swing: { path: 'assets/models/character/slash_swing.glb', type: 'gltf' },
      warrior_idle: { path: 'assets/models/character/warrior_idle.fbx', type: 'fbx' },
      tennis_idle: { path: 'assets/models/character/tennis_idle.fbx', type: 'fbx' }
    };

    const target = motionMap[motionKey] || motionMap.reap_swing;

    if (target.type === 'gltf') {
      const gltfLoader = new GLTFLoader();
      gltfLoader.load(target.path, (gltf) => {
        const char = gltf.scene;
        this.currentObject = char;
        char.traverse((c) => {
          if (c.isMesh) {
            c.castShadow = true;
            if (c.material) c.material.side = THREE.DoubleSide;
          }
        });
        this.scene.add(char);
        this.fitCamera(char, 1.3);

        if (gltf.animations && gltf.animations.length > 0) {
          this.mixer = new THREE.AnimationMixer(char);
          const act = this.mixer.clipAction(gltf.animations[0]);
          act.play();
        }
      });
    } else {
      const fbxLoader = new FBXLoader();
      fbxLoader.load(target.path, (charFbx) => {
        this.currentObject = charFbx;
        charFbx.scale.setScalar(0.015); // FBX 적절 스케일 보정
        charFbx.traverse((c) => {
          if (c.isMesh) {
            c.castShadow = true;
            if (c.material) c.material.side = THREE.DoubleSide;
          }
        });
        this.scene.add(charFbx);
        this.fitCamera(charFbx, 1.3);

        if (charFbx.animations && charFbx.animations.length > 0) {
          this.mixer = new THREE.AnimationMixer(charFbx);
          const act = this.mixer.clipAction(charFbx.animations[0]);
          act.play();
        }
      });
    }
  }

  setRacketColor(colorName) {
    if (!this.racketMeshes || this.racketMeshes.length === 0) return;
    const colorMap = {
      blue: 'assets/models/racket/Texture/racket_blue.png',
      red: 'assets/models/racket/Texture/racket_red.png',
      green: 'assets/models/racket/Texture/racket_green.png',
      orange: 'assets/models/racket/Texture/racket_orange.png',
      pink: 'assets/models/racket/Texture/racket_pink.png',
      yellow: 'assets/models/racket/Texture/racket_yellow.png'
    };
    const path = colorMap[colorName] || colorMap.blue;
    const tex = this.textureLoader.load(path);
    tex.colorSpace = THREE.SRGBColorSpace;

    this.racketMeshes.forEach((mesh) => {
      mesh.material.map = tex;
      mesh.material.needsUpdate = true;
    });
  }

  resize() {
    if (!this.container || !this.renderer || !this.camera) return;
    const w = this.container.clientWidth || 600;
    const h = this.container.clientHeight || 450;
    this.camera.aspect = w / h;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(w, h);
  }

  animate() {
    requestAnimationFrame(this.animate);
    const delta = this.clock.getDelta();

    if (this.mixer) {
      this.mixer.update(delta);
    }

    if (this.currentModelType === 'ball' && this.ballMesh && this.isBouncing) {
      this.ballTime += delta * 4;
      this.ballMesh.position.y = Math.abs(Math.sin(this.ballTime)) * 0.8;
      this.ballMesh.rotation.x += delta * 2;
    }

    this.controls.update();
    this.renderer.render(this.scene, this.camera);
  }
}

export { TennisIslandCinematicViewer, ModelInspectModalViewer };
