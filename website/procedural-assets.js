import * as THREE from 'three';

// 닌텐도 카툰 감성의 소프트 렌즈 글로우 스프라이트 텍스처 (Fake Glow Sprite)
export function createGlowSpriteTexture() {
  const canvas = document.createElement('canvas');
  canvas.width = 128;
  canvas.height = 128;
  const ctx = canvas.getContext('2d');

  const grad = ctx.createRadialGradient(64, 64, 0, 64, 64, 64);
  grad.addColorStop(0.0, 'rgba(255, 255, 255, 1.0)');
  grad.addColorStop(0.18, 'rgba(255, 255, 255, 0.85)');
  grad.addColorStop(0.45, 'rgba(255, 240, 200, 0.35)');
  grad.addColorStop(0.75, 'rgba(255, 200, 150, 0.08)');
  grad.addColorStop(1.0, 'rgba(255, 200, 150, 0)');

  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, 128, 128);

  const tex = new THREE.CanvasTexture(canvas);
  return tex;
}

/* =========================================================================
   0. 닌텐도 스포츠 감성 절차적 텍스처 & 3D 스타일라이즈드 에셋 생성기
========================================================================= */

// 닌텐도 특유의 화사한 2톤 체커보드 잔디 텍스처 (마리오 테니스/골프 리조트 룩)
export function createNintendoGrassTexture() {
  const canvas = document.createElement('canvas');
  canvas.width = 512;
  canvas.height = 512;
  const ctx = canvas.getContext('2d');

  // 1. 산뜻한 라임 그린 베이스
  ctx.fillStyle = '#4ade80';
  ctx.fillRect(0, 0, 512, 512);

  // 2. 경쾌한 체커보드 잔디 패턴 (마리오 테니스 스타일)
  const tileSize = 64;
  ctx.fillStyle = '#3ecc6b';
  for (let y = 0; y < 512; y += tileSize) {
    for (let x = 0; x < 512; x += tileSize) {
      if ((x / tileSize + y / tileSize) % 2 === 0) {
        ctx.fillRect(x, y, tileSize, tileSize);
      }
    }
  }

  // 3. 미세한 햇살 반사 풀잎 하이라이트
  ctx.fillStyle = '#86efac';
  for (let i = 0; i < 200; i++) {
    const rx = Math.random() * 512;
    const ry = Math.random() * 512;
    const rw = 2 + Math.random() * 3;
    const rh = 4 + Math.random() * 5;
    ctx.fillRect(rx, ry, rw, rh);
  }

  // 4. 귀여운 파스텔 미니 꽃 장식 (동물의 숲 / 닌텐도 리조트 디테일)
  for (let i = 0; i < 28; i++) {
    const fx = Math.random() * 512;
    const fy = Math.random() * 512;
    // 화이트 꽃잎
    ctx.fillStyle = '#ffffff';
    ctx.beginPath();
    ctx.arc(fx, fy, 2.6, 0, Math.PI * 2);
    ctx.fill();
    // 옐로우 꽃술
    ctx.fillStyle = '#fde047';
    ctx.beginPath();
    ctx.arc(fx, fy, 1.2, 0, Math.PI * 2);
    ctx.fill();
  }

  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.RepeatWrapping;
  texture.repeat.set(38, 38);
  texture.anisotropy = 8;
  return texture;
}

// 청량하고 맑은 에메랄드 리조트 수면 텍스처 (카툰 코스틱스 웨이브)
export function createResortWaterTexture() {
  const canvas = document.createElement('canvas');
  canvas.width = 512;
  canvas.height = 512;
  const ctx = canvas.getContext('2d');

  // 비비드 터콰이즈 & 코발트 그라디언트 베이스
  const grad = ctx.createLinearGradient(0, 0, 512, 512);
  grad.addColorStop(0, '#0284c7');
  grad.addColorStop(0.5, '#0ea5e9');
  grad.addColorStop(1, '#06b6d4');
  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, 512, 512);

  // 화사한 물결 하이라이트 (카툰풍 Caustics 웨이브)
  ctx.strokeStyle = 'rgba(255, 255, 255, 0.32)';
  ctx.lineWidth = 3.5;
  ctx.lineCap = 'round';

  for (let y = 15; y < 512; y += 45) {
    ctx.beginPath();
    ctx.moveTo(0, y);
    for (let x = 0; x <= 512; x += 40) {
      const cy = y + Math.sin((x / 40) * Math.PI) * 10;
      ctx.lineTo(x, cy);
    }
    ctx.stroke();
  }

  const texture = new THREE.CanvasTexture(canvas);
  texture.wrapS = THREE.RepeatWrapping;
  texture.wrapT = THREE.RepeatWrapping;
  texture.repeat.set(50, 50);
  texture.anisotropy = 8;
  return texture;
}

// 코트 중앙에 비치는 부드러운 스포트라이트 조명 풀(Glow) 텍스처 생성기
function createLightPoolTexture() {
  const canvas = document.createElement('canvas');
  canvas.width = 512;
  canvas.height = 512;
  const ctx = canvas.getContext('2d');

  const grad = ctx.createRadialGradient(256, 256, 0, 256, 256, 256);
  grad.addColorStop(0, 'rgba(255, 255, 255, 1.0)');
  grad.addColorStop(0.25, 'rgba(255, 255, 255, 0.85)');
  grad.addColorStop(0.55, 'rgba(255, 255, 255, 0.4)');
  grad.addColorStop(0.85, 'rgba(255, 255, 255, 0.08)');
  grad.addColorStop(1.0, 'rgba(255, 255, 255, 0)');

  ctx.fillStyle = grad;
  ctx.fillRect(0, 0, 512, 512);

  const tex = new THREE.CanvasTexture(canvas);
  tex.wrapS = THREE.ClampToEdgeWrapping;
  tex.wrapT = THREE.ClampToEdgeWrapping;
  return tex;
}

// 푹신하고 귀여운 닌텐도 스타일 솜사탕 구름 생성기
export function createStylizedCloud() {
  const cloud = new THREE.Group();
  const cloudMat = new THREE.MeshStandardMaterial({
    color: 0xffffff,
    roughness: 0.25,
    metalness: 0.05,
    flatShading: true
  });

  const puffGeo = new THREE.DodecahedronGeometry(14, 1);
  const numPuffs = 4 + Math.floor(Math.random() * 3);

  for (let i = 0; i < numPuffs; i++) {
    const puff = new THREE.Mesh(puffGeo, cloudMat);
    const px = (i - numPuffs / 2) * 13 + (Math.random() - 0.5) * 6;
    const py = (Math.random() - 0.5) * 6;
    const pz = (Math.random() - 0.5) * 8;
    const s = 0.85 + Math.random() * 0.55;
    puff.position.set(px, py, pz);
    puff.scale.set(s * 1.25, s * 0.85, s * 1.15);
    puff.castShadow = true;
    cloud.add(puff);
  }
  return cloud;
}

/* =========================================================================
   1. ACE DOME: 거대 돔 형식 외벽 껍데기 모델링
   (기존 관중석 외벽 깨짐/사각 모서리 완전 커버 & 웅장한 스포츠 돔 건축)
========================================================================= */
export function addGrandAceDomeShell(stadiumUnit) {
  const domeGroup = new THREE.Group();
  domeGroup.name = 'grand_ace_dome_shell';

  const segments = 64;

  // 1. 하단 광장 포디엄 외벽 튜브 (안쪽이 뻥 뚫린 링 형태: 경기장 내부 하얀 바닥 차단)
  const plazaGeo = new THREE.CylinderGeometry(73, 76, 1.8, segments, 1, true);
  const plazaMat = new THREE.MeshStandardMaterial({
    color: 0xf1f5f9,
    roughness: 0.65,
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

  // 2. 돔 하부 유선형 외벽 (세로 높이 28로 시원하게 증대: 위풍당당한 콜로세움 돔 파사드)
  const lowerWallGeo = new THREE.CylinderGeometry(67, 72, 28, segments, 1, true);
  const whitePanelMat = new THREE.MeshStandardMaterial({
    color: 0xf8fafc,
    roughness: 0.28,
    metalness: 0.08,
    side: THREE.DoubleSide
  });
  const lowerWall = new THREE.Mesh(lowerWallGeo, whitePanelMat);
  lowerWall.position.y = 15.8;
  lowerWall.castShadow = true;
  lowerWall.receiveShadow = true;
  domeGroup.add(lowerWall);

  // 3. 시그니처 블루 리본 밴드 (높이 6.0) - 닌텐도 스위치 네온 블루
  const ribbonGeo = new THREE.CylinderGeometry(67.8, 68.2, 6.0, segments, 1, true);
  const blueRibbonMat = new THREE.MeshStandardMaterial({
    color: 0x0088ff,
    roughness: 0.22,
    metalness: 0.18,
    side: THREE.DoubleSide
  });
  const ribbon = new THREE.Mesh(ribbonGeo, blueRibbonMat);
  ribbon.position.y = 25.0;
  domeGroup.add(ribbon);

  // 4. 발광 시안 LED 엣지 링 (원경 가시성을 위해 두께 최적화 및 상단 오큘러스 림 추가)
  const ledGeo = new THREE.TorusGeometry(68.0, 0.75, 8, segments);
  const ledMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8 });
  const ledTop = new THREE.Mesh(ledGeo, ledMat);
  ledTop.name = 'stadium_led_ring';
  ledTop.rotation.x = Math.PI / 2;
  ledTop.position.y = 28.2;
  domeGroup.add(ledTop);

  const ledBot = new THREE.Mesh(ledGeo, ledMat);
  ledBot.name = 'stadium_led_ring';
  ledBot.rotation.x = Math.PI / 2;
  ledBot.position.y = 21.8;
  domeGroup.add(ledBot);

  const ledOculusGeo = new THREE.TorusGeometry(43.8, 0.7, 8, segments);
  const ledOculus = new THREE.Mesh(ledOculusGeo, ledMat);
  ledOculus.name = 'stadium_led_ring';
  ledOculus.rotation.x = Math.PI / 2;
  ledOculus.position.y = 50.6;
  domeGroup.add(ledOculus);

  // 5. 돔 상부 완만한 유선형 지붕 (원형 오큘러스 개구부로 연결, y: 29.8 ~ 50.5)
  // 티어 1: 반지름 67.8 -> 60, 높이 8.0
  const domeTier1Geo = new THREE.CylinderGeometry(60, 67.8, 8.0, segments, 1, true);
  const domeTier1 = new THREE.Mesh(domeTier1Geo, whitePanelMat);
  domeTier1.position.y = 33.8;
  domeTier1.castShadow = true;
  domeTier1.receiveShadow = true;
  domeGroup.add(domeTier1);

  // 티어 2: 반지름 60 -> 50, 높이 8.0
  const domeTier2Geo = new THREE.CylinderGeometry(50, 60, 8.0, segments, 1, true);
  const domeTier2 = new THREE.Mesh(domeTier2Geo, whitePanelMat);
  domeTier2.position.y = 41.8;
  domeTier2.castShadow = true;
  domeTier2.receiveShadow = true;
  domeGroup.add(domeTier2);

  // 티어 3: 반지름 50 -> 43, 높이 4.5 (상단 거대 원형 구멍 림)
  const domeTier3Geo = new THREE.CylinderGeometry(43, 50, 4.5, segments, 1, true);
  const domeTier3 = new THREE.Mesh(domeTier3Geo, whitePanelMat);
  domeTier3.position.y = 48.0;
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
  oculus.position.y = 50.5;
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
  glass.position.y = 50.48;
  domeGroup.add(glass);

  // 7. 돔 외곽 원기둥 수직 건축 기둥 루버 (높이 29.0으로 시원하게 증대)
  const colGeo = new THREE.BoxGeometry(1.1, 29.0, 2.5);
  const colMat = new THREE.MeshStandardMaterial({
    color: 0x0284c7,
    roughness: 0.45,
    metalness: 0.3
  });

  for (let i = 0; i < 36; i++) {
    const angle = (i / 36) * Math.PI * 2;
    const col = new THREE.Mesh(colGeo, colMat);
    const rad = 70.8;
    col.position.set(Math.cos(angle) * rad, 15.8, Math.sin(angle) * rad);
    col.rotation.y = -angle + Math.PI / 2;
    col.castShadow = true;
    domeGroup.add(col);
  }

  // 8. 4대 메인 출입구 파빌리온 (높이 12.0)
  const gateGeo = new THREE.BoxGeometry(6.0, 12.0, 4.0);
  const gateMat = new THREE.MeshStandardMaterial({ color: 0x0077e6, roughness: 0.4 });
  const gateGlassMat = new THREE.MeshBasicMaterial({ color: 0x38bdf8, transparent: true, opacity: 0.8 });

  for (let i = 0; i < 4; i++) {
    const angle = (i / 4) * Math.PI * 2;
    const gate = new THREE.Group();
    const frame = new THREE.Mesh(gateGeo, gateMat);
    frame.position.y = 6.0;
    const innerGlass = new THREE.Mesh(new THREE.PlaneGeometry(4.8, 10.0), gateGlassMat);
    innerGlass.position.set(0, 5.8, 2.01);

    gate.add(frame);
    gate.add(innerGlass);
    const grad = 71.8;
    gate.position.set(Math.cos(angle) * grad, 0, Math.sin(angle) * grad);
    gate.rotation.y = -angle + Math.PI / 2;
    domeGroup.add(gate);
  }

  stadiumUnit.add(domeGroup);
}

/* =========================================================================
   2. 닌텐도 스위치 스포츠 감성 알록달록 하늘 풍선 (Floating Balloons)
========================================================================= */
export function createFloatingBalloonsGroup() {
  const balloonsRoot = new THREE.Group();
  balloonsRoot.name = 'floating_balloons_root';

  const balloonColors = [
    0xff3366, // 비비드 스트로베리
    0xffbb00, // 골든 옐로우
    0x00d4ff, // 맑은 스카이블루
    0x2ec4b6, // 에메랄드 민트
    0xff70a6, // 파스텔 핑크
    0x8338ec, // 바이올렛
    0xff5400, // 네온 오렌지
    0x70e000  // 라임 그린
  ];

  const balloonSphereGeo = new THREE.SphereGeometry(2.4, 16, 14);
  balloonSphereGeo.scale(1.0, 1.28, 1.0); // 계란형 볼륨

  const knotGeo = new THREE.ConeGeometry(0.55, 0.65, 8);

  const stringMat = new THREE.LineBasicMaterial({
    color: 0xffffff,
    transparent: true,
    opacity: 0.75
  });

  const balloonItems = [];

  function createSingleBalloon(colorHex, scale = 1.0) {
    const single = new THREE.Group();
    const mat = new THREE.MeshStandardMaterial({
      color: colorHex,
      roughness: 0.22,
      metalness: 0.15,
      emissive: colorHex,
      emissiveIntensity: 0.12
    });

    const body = new THREE.Mesh(balloonSphereGeo, mat);
    body.castShadow = true;
    single.add(body);

    const knot = new THREE.Mesh(knotGeo, mat);
    knot.rotation.x = Math.PI;
    knot.position.y = -3.0;
    single.add(knot);

    // 살랑거리는 실 (String)
    const points = [
      new THREE.Vector3(0, -3.2, 0),
      new THREE.Vector3(0.3, -4.8, 0.1),
      new THREE.Vector3(-0.2, -6.5, -0.1),
      new THREE.Vector3(0.1, -8.2, 0.15)
    ];
    const stringGeo = new THREE.BufferGeometry().setFromPoints(points);
    const line = new THREE.Line(stringGeo, stringMat);
    single.add(line);

    single.scale.setScalar(scale);
    return single;
  }

  // 풍선 사이사이 간격을 넉넉히 띄워 겹치지 않게 섬 하늘 전역에 분산 배치 (최소 거리 28 유닛 이상 확보)
  const wellSpacedAnchors = [
    { x: -165, y: 72, z: 70 },
    { x: -130, y: 96, z: -60 },
    { x: -195, y: 82, z: -25 },
    { x: 95,   y: 76, z: -165 },
    { x: 155,  y: 94, z: -105 },
    { x: 175,  y: 112, z: -175 },
    { x: 95,   y: 74, z: 165 },
    { x: 155,  y: 95, z: 115 },
    { x: 175,  y: 108, z: 185 },
    { x: 0,    y: 88, z: 0 },
    { x: -45,  y: 102, z: 75 },
    { x: 45,   y: 82, z: -65 },
    { x: -85,  y: 108, z: -135 },
    { x: 85,   y: 80, z: 65 },
    { x: -115, y: 92, z: 145 },
    { x: 55,   y: 108, z: 145 },
    { x: -75,  y: 84, z: -45 },
    { x: 95,   y: 98, z: -45 },
    { x: -215, y: 90, z: 35 },
    { x: 205,  y: 88, z: 75 },
    { x: -35,  y: 118, z: -95 },
    { x: 35,   y: 115, z: 95 }
  ];

  wellSpacedAnchors.forEach((pos, idx) => {
    const color = balloonColors[idx % balloonColors.length];
    const b = createSingleBalloon(color, 1.05 + (idx % 3) * 0.16);
    b.position.set(pos.x, pos.y, pos.z);
    b.rotation.z = (Math.random() - 0.5) * 0.18;
    balloonsRoot.add(b);

    balloonItems.push({
      group: b,
      baseY: pos.y,
      speed: 0.9 + (idx % 5) * 0.18,
      phase: idx * 0.85
    });
  });

  return {
    group: balloonsRoot,
    update(elapsed) {
      balloonItems.forEach((item) => {
        // 부드럽게 오르내리는 부력 바운스 & 바람에 살랑이는 흔들림
        item.group.position.y = item.baseY + Math.sin(elapsed * item.speed + item.phase) * 3.8;
        item.group.rotation.z = Math.sin(elapsed * (item.speed * 0.75) + item.phase) * 0.12;
        item.group.rotation.x = Math.cos(elapsed * (item.speed * 0.6) + item.phase) * 0.08;
      });
    }
  };
}

/* =========================================================================
   3. 하늘에 쉼 없이 펑펑 터지는 축제 폭죽 시스템 (Continuous Fireworks System)
========================================================================= */
export function createFestiveFireworksSystem() {
  const fireworksRoot = new THREE.Group();
  fireworksRoot.name = 'festive_fireworks_root';

  // 불꽃 입자 텍스처 (중심은 눈부시게 밝고 외곽은 부드럽게 퍼지는 글로우 스파크)
  const sparkCanvas = document.createElement('canvas');
  sparkCanvas.width = 64;
  sparkCanvas.height = 64;
  const sCtx = sparkCanvas.getContext('2d');
  const grad = sCtx.createRadialGradient(32, 32, 0, 32, 32, 32);
  grad.addColorStop(0.0, 'rgba(255, 255, 255, 1.0)');
  grad.addColorStop(0.2, 'rgba(255, 255, 255, 0.95)');
  grad.addColorStop(0.55, 'rgba(255, 240, 200, 0.45)');
  grad.addColorStop(1.0, 'rgba(255, 240, 200, 0.0)');
  sCtx.fillStyle = grad;
  sCtx.fillRect(0, 0, 64, 64);
  const sparkTex = new THREE.CanvasTexture(sparkCanvas);

  const fireworkColors = [
    new THREE.Color(0xffd700), // 샴페인 골드
    new THREE.Color(0x00f5ff), // 일렉트릭 시안
    new THREE.Color(0xff007f), // 네온 마젠타
    new THREE.Color(0x39ff14), // 에메랄드 라임
    new THREE.Color(0xff5500), // 비비드 오렌지
    new THREE.Color(0xbf00ff), // 로얄 바이올렛
    new THREE.Color(0x38bdf8), // 맑은 스카이 블루
    new THREE.Color(0xffffff), // 퓨어 다이아몬드 화이트
    new THREE.Color(0xff2a85), // 핫 핑크
    new THREE.Color(0xfacc15), // 비비드 옐로우
  ];

  // 동시 활성화 폭죽 슬롯 (7개 동시 순환 연쇄 폭발)
  const BURST_COUNT = 7;
  const PARTICLES_PER_BURST = 85;

  const bursts = [];

  for (let b = 0; b < BURST_COUNT; b++) {
    const geo = new THREE.BufferGeometry();
    const posArr = new Float32Array(PARTICLES_PER_BURST * 3);
    const colArr = new Float32Array(PARTICLES_PER_BURST * 3);

    geo.setAttribute('position', new THREE.BufferAttribute(posArr, 3));
    geo.setAttribute('color', new THREE.BufferAttribute(colArr, 3));

    const mat = new THREE.PointsMaterial({
      size: 7.2,
      map: sparkTex,
      vertexColors: true,
      transparent: true,
      opacity: 0.0,
      blending: THREE.AdditiveBlending,
      depthWrite: false
    });

    const pointsMesh = new THREE.Points(geo, mat);
    fireworksRoot.add(pointsMesh);

    const velocities = [];
    for (let i = 0; i < PARTICLES_PER_BURST; i++) {
      velocities.push(new THREE.Vector3());
    }

    const burstObj = {
      mesh: pointsMesh,
      geo,
      mat,
      posArr,
      colArr,
      velocities,
      center: new THREE.Vector3(),
      life: 0,
      maxLife: 1.8 + Math.random() * 0.5,
      timer: (b / BURST_COUNT) * 1.6, // 시간차를 두고 계속 펑펑 터지도록 스태거 오프셋
      active: false
    };

    bursts.push(burstObj);
  }

  function triggerBurst(burst) {
    // 3대 경기장 및 중앙 해변, 언덕 상공의 다채로운 타겟 좌표
    const targetSpots = [
      { x: -160, z: -10 }, // 테니스 경기장 상공
      { x: -130, z: 40 },  // 테니스 앞바다 상공
      { x: 130,  z: -150 },// 볼링 경기장 상공
      { x: 90,   z: -80 }, // 볼링-중앙 사이 상공
      { x: 130,  z: 150 }, // 검술 아레나 상공
      { x: 80,   z: 90 },  // 검술 해안 상공
      { x: 0,    z: -60 }, // 중앙 만(bay) 요트 상공
      { x: -20,  z: 120 }, // 남쪽 리조트 빌라 상공
    ];
    const spot = targetSpots[Math.floor(Math.random() * targetSpots.length)];
    const cx = spot.x + (Math.random() - 0.5) * 70;
    const cy = 130 + Math.random() * 110; // 높이 130 ~ 240
    const cz = spot.z + (Math.random() - 0.5) * 70;
    burst.center.set(cx, cy, cz);

    const primaryColor = fireworkColors[Math.floor(Math.random() * fireworkColors.length)];
    const secondaryColor = fireworkColors[Math.floor(Math.random() * fireworkColors.length)];

    for (let i = 0; i < PARTICLES_PER_BURST; i++) {
      burst.posArr[i * 3] = cx;
      burst.posArr[i * 3 + 1] = cy;
      burst.posArr[i * 3 + 2] = cz;

      // 구형 3D 방사 속도
      const phi = Math.random() * Math.PI * 2;
      const cosTheta = Math.random() * 2 - 1;
      const sinTheta = Math.sqrt(1 - cosTheta * cosTheta);
      const speed = 24 + Math.random() * 28;

      burst.velocities[i].set(
        sinTheta * Math.cos(phi) * speed,
        cosTheta * speed + 8.0, // 약간의 상향 추진력
        sinTheta * Math.sin(phi) * speed
      );

      // 투톤 컬러 블렌딩
      const mixRatio = Math.random();
      const sparkColor = primaryColor.clone().lerp(secondaryColor, mixRatio);
      burst.colArr[i * 3] = sparkColor.r;
      burst.colArr[i * 3 + 1] = sparkColor.g;
      burst.colArr[i * 3 + 2] = sparkColor.b;
    }

    burst.geo.attributes.position.needsUpdate = true;
    burst.geo.attributes.color.needsUpdate = true;
    burst.life = 0;
    burst.active = true;
    burst.mat.opacity = 1.0;
  }

  return {
    group: fireworksRoot,
    update(delta) {
      bursts.forEach((b) => {
        if (!b.active) {
          b.timer -= delta;
          if (b.timer <= 0) {
            triggerBurst(b);
          }
        } else {
          b.life += delta;
          const progress = b.life / b.maxLife;

          if (progress >= 1.0) {
            b.active = false;
            b.mat.opacity = 0.0;
            b.timer = 0.15 + Math.random() * 0.55; // 잠시 후 다시 연속 폭발!
          } else {
            // 페이드 아웃 & 반짝이는 트윙클
            const alpha = 1.0 - progress * progress;
            const flicker = 0.88 + Math.sin(b.life * 32) * 0.12;
            b.mat.opacity = Math.max(0, alpha * flicker);
            b.mat.size = 7.2 * (1.0 - progress * 0.35);

            for (let i = 0; i < PARTICLES_PER_BURST; i++) {
              // 중력 & 공기 저항
              b.velocities[i].y -= 9.8 * 1.7 * delta;
              b.velocities[i].x *= 0.976;
              b.velocities[i].z *= 0.976;

              b.posArr[i * 3] += b.velocities[i].x * delta;
              b.posArr[i * 3 + 1] += b.velocities[i].y * delta;
              b.posArr[i * 3 + 2] += b.velocities[i].z * delta;
            }
            b.geo.attributes.position.needsUpdate = true;
          }
        }
      });
    }
  };
}

/* =========================================================================
   4. 바람에 살랑살랑 흔들리는 조그마한 3D 잔디 풀밭 시스템 (Instanced Grass Tufts)
========================================================================= */
export function createSwayingGrassSystem(stadiumCenters = [], isRoadCollision = null) {
  const geom = new THREE.BufferGeometry();
  const positions = [];
  const normals = [];
  const uvs = [];

  const bladeCount = 3;
  for (let b = 0; b < bladeCount; b++) {
    const angle = (b / bladeCount) * Math.PI;
    const cos = Math.cos(angle);
    const sin = Math.sin(angle);
    const w = 0.45;
    const h = 1.7;

    // Triangle 1
    positions.push(-w * cos, 0, -w * sin);
    normals.push(-sin, 0.2, cos);
    uvs.push(0, 0);

    positions.push(w * cos, 0, w * sin);
    normals.push(-sin, 0.2, cos);
    uvs.push(1, 0);

    positions.push(0.12 * cos, h, 0.12 * sin);
    normals.push(-sin, 0.2, cos);
    uvs.push(0.5, 1);

    // Triangle 2 (back face)
    positions.push(w * cos, 0, w * sin);
    normals.push(sin, 0.2, -cos);
    uvs.push(1, 0);

    positions.push(-w * cos, 0, -w * sin);
    normals.push(sin, 0.2, -cos);
    uvs.push(0, 0);

    positions.push(0.12 * cos, h, 0.12 * sin);
    normals.push(sin, 0.2, -cos);
    uvs.push(0.5, 1);
  }

  geom.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
  geom.setAttribute('normal', new THREE.Float32BufferAttribute(normals, 3));
  geom.setAttribute('uv', new THREE.Float32BufferAttribute(uvs, 2));

  // Grass Material with GPU Vertex Wind Animation
  const grassMat = new THREE.MeshStandardMaterial({
    color: 0x42b22e,
    roughness: 0.72,
    metalness: 0.04,
    side: THREE.DoubleSide
  });

  grassMat.onBeforeCompile = (shader) => {
    shader.uniforms.uTime = { value: 0 };
    shader.vertexShader = `
      uniform float uTime;
      \${shader.vertexShader}
    `;
    shader.vertexShader = shader.vertexShader.replace(
      '#include <begin_vertex>',
      `
      #include <begin_vertex>
      float h = max(0.0, transformed.y);
      vec4 worldPos = modelMatrix * vec4(position, 1.0);
      float sway = sin(uTime * 2.6 + worldPos.x * 0.1 + worldPos.z * 0.12) * 0.35 * h;
      float swayZ = cos(uTime * 2.0 + worldPos.x * 0.12 + worldPos.z * 0.09) * 0.25 * h;
      transformed.x += sway;
      transformed.z += swayZ;
      `
    );
    grassMat.userData.shader = shader;
  };

  const TOTAL_TUFTS = 1400;
  const instancedMesh = new THREE.InstancedMesh(geom, grassMat, TOTAL_TUFTS);
  instancedMesh.receiveShadow = true;

  const dummy = new THREE.Object3D();
  let count = 0;

  for (let i = 0; i < TOTAL_TUFTS * 2 && count < TOTAL_TUFTS; i++) {
    const angle = Math.random() * Math.PI * 2;
    const r = 25 + Math.random() * 355;
    const tx = Math.cos(angle) * r;
    const tz = Math.sin(angle) * r;

    // 경기장 충돌 회피
    let collides = false;
    for (const sc of stadiumCenters) {
      if (Math.hypot(tx - sc.x, tz - sc.z) < sc.radius) {
        collides = true;
        break;
      }
    }
    if (collides) continue;

    // 도로 충돌 회피 (주황색 도로 위에는 잔디가 돋지 않도록)
    if (isRoadCollision && isRoadCollision(tx, tz)) continue;

    dummy.position.set(tx, 0.0, tz);
    const scale = 0.85 + Math.random() * 0.7;
    dummy.scale.set(scale, scale * (0.9 + Math.random() * 0.35), scale);
    dummy.rotation.y = Math.random() * Math.PI * 2;
    dummy.rotation.x = (Math.random() - 0.5) * 0.12;
    dummy.rotation.z = (Math.random() - 0.5) * 0.12;
    dummy.updateMatrix();

    instancedMesh.setMatrixAt(count, dummy.matrix);
    count++;
  }

  instancedMesh.instanceMatrix.needsUpdate = true;

  return {
    mesh: instancedMesh,
    update(elapsed) {
      if (grassMat.userData.shader) {
        grassMat.userData.shader.uniforms.uTime.value = elapsed;
      }
    }
  };
}

/* =========================================================================
   5. 경기장 간 연결 주황색 도로 & 가로등 & 가로수 시스템
   (Orange Connecting Pathways, Street Lamps, and Roadside Palm Trees)
========================================================================= */

// 주황색 스포츠 리조트 트랙 캔버스 텍스처 생성기
export function createResortRoadTexture() {
  const canvas = document.createElement('canvas');
  canvas.width = 128;
  canvas.height = 512;
  const ctx = canvas.getContext('2d');

  // 1. 선명하고 산뜻한 스포츠 리조트 오렌지 트랙 베이스
  ctx.fillStyle = '#ea580c'; // 짙은 테라코타 오렌지
  ctx.fillRect(0, 0, 128, 512);

  // 안쪽 주황색 러닝 트랙 본체
  ctx.fillStyle = '#f97316';
  ctx.fillRect(8, 0, 112, 512);

  // 은은한 트랙 하이라이트 틴트
  ctx.fillStyle = '#fb923c';
  ctx.fillRect(16, 0, 96, 512);

  // 2. 양쪽 사이드 화이트 라인 (달리기 트랙 / 산책로 경계선)
  ctx.fillStyle = '#ffffff';
  ctx.fillRect(12, 0, 5, 512);
  ctx.fillRect(111, 0, 5, 512);

  // 3. 중앙 화이트 점선 (Dashed Center Line)
  ctx.fillStyle = '#ffffff';
  const dashLen = 34;
  const gapLen = 30;
  for (let y = 0; y < 512; y += dashLen + gapLen) {
    ctx.fillRect(61, y, 6, dashLen);
  }

  const tex = new THREE.CanvasTexture(canvas);
  tex.wrapS = THREE.RepeatWrapping;
  tex.wrapT = THREE.RepeatWrapping;
  return tex;
}

export function createResortPathwaysSystem() {
  const pathwaysGroup = new THREE.Group();
  pathwaysGroup.name = 'resort_pathways_system';

  const roadTex = createResortRoadTexture();
  const roadMat = new THREE.MeshStandardMaterial({
    map: roadTex,
    roughness: 0.65,
    metalness: 0.05,
    side: THREE.DoubleSide
  });

  const plazaMat = new THREE.MeshStandardMaterial({
    color: 0xf97316,
    roughness: 0.65,
    metalness: 0.05,
    side: THREE.DoubleSide
  });

  const curbMat = new THREE.MeshStandardMaterial({
    color: 0xf8fafc,
    roughness: 0.4,
    metalness: 0.08,
    side: THREE.DoubleSide
  });

  const lampMetalMat = new THREE.MeshStandardMaterial({
    color: 0x1e293b,
    roughness: 0.35,
    metalness: 0.85
  });

  const lampBulbMat = new THREE.MeshStandardMaterial({
    color: 0xffffff,
    emissive: 0xfffae6,
    emissiveIntensity: 0.05,
    roughness: 0.15,
    metalness: 0.1
  });

  const glowTex = createGlowSpriteTexture();

  const streetLamps = [];
  const roadsideTreePositions = [];
  const sampledPathPoints = []; // 충돌 검사용 2D 샘플 포인트

  // 1. 중앙 원형 로터리 광장 (Central Roundabout Plaza, radius 14 ~ 25)
  const plazaRoadGeo = new THREE.RingGeometry(14, 25, 48);
  const plazaRoad = new THREE.Mesh(plazaRoadGeo, plazaMat);
  plazaRoad.rotation.x = -Math.PI / 2;
  plazaRoad.position.y = 0.32;
  plazaRoad.receiveShadow = true;
  pathwaysGroup.add(plazaRoad);

  // 로터리 외곽/내측 화이트 연석
  const outerCurbGeo = new THREE.RingGeometry(24.8, 25.8, 48);
  const outerCurb = new THREE.Mesh(outerCurbGeo, curbMat);
  outerCurb.rotation.x = -Math.PI / 2;
  outerCurb.position.y = 0.42;
  pathwaysGroup.add(outerCurb);

  const innerCurbGeo = new THREE.RingGeometry(13.2, 14.2, 48);
  const innerCurb = new THREE.Mesh(innerCurbGeo, curbMat);
  innerCurb.rotation.x = -Math.PI / 2;
  innerCurb.position.y = 0.42;
  pathwaysGroup.add(innerCurb);

  // 로터리 중앙 잔디 정원 (Center Garden Island)
  const centerGardenGeo = new THREE.CircleGeometry(13.2, 32);
  const centerGardenMat = new THREE.MeshStandardMaterial({
    color: 0x3ea82a,
    roughness: 0.75,
    side: THREE.DoubleSide
  });
  const centerGarden = new THREE.Mesh(centerGardenGeo, centerGardenMat);
  centerGarden.rotation.x = -Math.PI / 2;
  centerGarden.position.y = 0.33;
  pathwaysGroup.add(centerGarden);

  // 중앙 정원 가로수 야자수 4그루 (센터포인트 랜드마크)
  const gardenAngles = [0, Math.PI * 0.5, Math.PI, Math.PI * 1.5];
  gardenAngles.forEach((ang) => {
    roadsideTreePositions.push(new THREE.Vector3(Math.cos(ang) * 6.5, 0, Math.sin(ang) * 6.5));
  });

  // 2. 가로등 생성 헬퍼 함수
  const poleGeo = new THREE.CylinderGeometry(0.14, 0.22, 6.4, 10);
  const baseGeo = new THREE.CylinderGeometry(0.42, 0.58, 0.45, 10);
  const armGeo = new THREE.BoxGeometry(0.14, 0.14, 1.4);
  const hoodGeo = new THREE.ConeGeometry(0.75, 0.4, 10);
  const bulbGeo = new THREE.SphereGeometry(0.38, 12, 12);

  function addStreetLamp(x, z, rotY = 0, hasLight = false) {
    const lamp = new THREE.Group();
    lamp.position.set(x, 0, z);
    lamp.rotation.y = rotY;

    // 기둥 베이스
    const base = new THREE.Mesh(baseGeo, lampMetalMat);
    base.position.y = 0.22;
    base.castShadow = true;
    lamp.add(base);

    // 수직 기둥
    const pole = new THREE.Mesh(poleGeo, lampMetalMat);
    pole.position.y = 3.4;
    pole.castShadow = true;
    lamp.add(pole);

    // 상단 암 (도로 쪽으로 뻗은 가로대)
    const arm = new THREE.Mesh(armGeo, lampMetalMat);
    arm.position.set(0, 6.4, 0.65);
    lamp.add(arm);

    // 램프 갓
    const hood = new THREE.Mesh(hoodGeo, lampMetalMat);
    hood.position.set(0, 6.2, 1.3);
    lamp.add(hood);

    // 발광 램프 전구
    const bulb = new THREE.Mesh(bulbGeo, lampBulbMat);
    bulb.position.set(0, 5.85, 1.3);
    lamp.add(bulb);

    // 렌즈 글로우 스프라이트
    const glowMat = new THREE.SpriteMaterial({
      map: glowTex,
      color: 0xffe680,
      transparent: true,
      opacity: 0.0,
      blending: THREE.AdditiveBlending,
      depthWrite: false
    });
    const glowSprite = new THREE.Sprite(glowMat);
    glowSprite.position.set(0, 5.85, 1.3);
    glowSprite.scale.set(11.0, 11.0, 1.0);
    lamp.add(glowSprite);

    let light = null;
    if (hasLight) {
      light = new THREE.PointLight(0xffe082, 0.0, 48, 1.2);
      light.position.set(0, 5.8, 1.3);
      lamp.add(light);
    }

    pathwaysGroup.add(lamp);

    streetLamps.push({
      group: lamp,
      sprite: glowSprite,
      glowMat: glowMat,
      light: light
    });
  }

  // 로터리 4방향 가로등
  for (let i = 0; i < 4; i++) {
    const ang = (i / 4) * Math.PI * 2 + Math.PI * 0.25;
    const lx = Math.cos(ang) * 26.5;
    const lz = Math.sin(ang) * 26.5;
    addStreetLamp(lx, lz, ang + Math.PI, true); // PointLight 장착
  }

  // 3. 부드러운 곡선 도로 생성 헬퍼 함수
  function buildRibbonMesh(rawPoints, width = 13.0) {
    const points3D = rawPoints.map(p => new THREE.Vector3(p.x, 0, p.z));
    const curve = new THREE.CatmullRomCurve3(points3D);
    const length = curve.getLength();
    const segments = Math.max(16, Math.floor(length / 2.5));

    const positions = [];
    const uvs = [];
    const indices = [];

    const curbPositions = [];
    const curbUvs = [];
    const curbIndices = [];

    const routeSampled = [];

    for (let i = 0; i <= segments; i++) {
      const t = i / segments;
      const pt = curve.getPoint(t);
      const tangent = curve.getTangent(t).normalize();
      const norm = new THREE.Vector3(-tangent.z, 0, tangent.x).normalize();

      routeSampled.push({ pt, norm });
      sampledPathPoints.push(pt);

      const halfW = width * 0.5;
      const leftX = pt.x + norm.x * halfW;
      const leftZ = pt.z + norm.z * halfW;
      const rightX = pt.x - norm.x * halfW;
      const rightZ = pt.z - norm.z * halfW;

      const y = 0.32; // grass(0.0)보다 확실히 위에 배치하여 잔디 파묻힘 완벽 방지

      positions.push(leftX, y, leftZ);
      positions.push(rightX, y, rightZ);

      const v = (t * length) / 22.0;
      uvs.push(0, v);
      uvs.push(1, v);

      // 연석 (Curb, 높이 y = 0.42)
      const curbY = 0.42;
      const curbW = 0.8;
      const curbOutLeftX = leftX + norm.x * curbW;
      const curbOutLeftZ = leftZ + norm.z * curbW;
      curbPositions.push(curbOutLeftX, curbY, curbOutLeftZ);
      curbPositions.push(leftX, curbY, leftZ);
      curbUvs.push(0, v);
      curbUvs.push(1, v);

      const curbOutRightX = rightX - norm.x * curbW;
      const curbOutRightZ = rightZ - norm.z * curbW;
      curbPositions.push(rightX, curbY, rightZ);
      curbPositions.push(curbOutRightX, curbY, curbOutRightZ);
      curbUvs.push(0, v);
      curbUvs.push(1, v);
    }

    for (let i = 0; i < segments; i++) {
      const a = i * 2;
      const b = i * 2 + 1;
      const c = (i + 1) * 2;
      const d = (i + 1) * 2 + 1;

      // ★★★ 상단(+Y)을 향하는 올바른 카운터클락와이즈(CCW) 와인딩 오더 ★★★
      indices.push(a, c, b);
      indices.push(b, c, d);

      // Left curb
      const ca = i * 4;
      const cb = i * 4 + 1;
      const cc = (i + 1) * 4;
      const cd = (i + 1) * 4 + 1;
      curbIndices.push(ca, cc, cb);
      curbIndices.push(cb, cc, cd);

      // Right curb
      const cra = i * 4 + 2;
      const crb = i * 4 + 3;
      const crc = (i + 1) * 4 + 2;
      const crd = (i + 1) * 4 + 3;
      curbIndices.push(cra, crc, crb);
      curbIndices.push(crb, crc, crd);
    }

    const roadGeo = new THREE.BufferGeometry();
    roadGeo.setAttribute('position', new THREE.Float32BufferAttribute(positions, 3));
    roadGeo.setAttribute('uv', new THREE.Float32BufferAttribute(uvs, 2));
    roadGeo.setIndex(indices);
    roadGeo.computeVertexNormals();

    const roadMesh = new THREE.Mesh(roadGeo, roadMat);
    roadMesh.receiveShadow = true;
    pathwaysGroup.add(roadMesh);

    const curbGeo = new THREE.BufferGeometry();
    curbGeo.setAttribute('position', new THREE.Float32BufferAttribute(curbPositions, 3));
    curbGeo.setAttribute('uv', new THREE.Float32BufferAttribute(curbUvs, 2));
    curbGeo.setIndex(curbIndices);
    curbGeo.computeVertexNormals();

    const curbMesh = new THREE.Mesh(curbGeo, curbMat);
    curbMesh.receiveShadow = true;
    pathwaysGroup.add(curbMesh);

    return { routeSampled, length };
  }

  // 4. 6대 핵심 연결 도로 경로 정의
  const routesData = [
    // 1) 센터 로터리 ➔ 테니스 경기장 진입로
    [
      { x: -23, z: 0 },
      { x: -52, z: 0 },
      { x: -82, z: 0 }
    ],
    // 2) 센터 로터리 ➔ 볼링 경기장 진입로
    [
      { x: 16, z: -19 },
      { x: 45, z: -55 },
      { x: 75, z: -95 }
    ],
    // 3) 센터 로터리 ➔ 검술 경기장 진입로
    [
      { x: 16, z: 19 },
      { x: 45, z: 55 },
      { x: 75, z: 95 }
    ],
    // 4) 볼링 ➔ 검술 동쪽 해안 직통 대로 (Eastern Boulevard)
    [
      { x: 130, z: -72 },
      { x: 146, z: -35 },
      { x: 152, z: 0 },
      { x: 146, z: 35 },
      { x: 130, z: 72 }
    ],
    // 5) 테니스 ➔ 볼링 북쪽 해안선 곡선 대로 (Northern Coastal Curve)
    [
      { x: -105, z: -55 },
      { x: -75, z: -105 },
      { x: -20, z: -145 },
      { x: 20, z: -158 },
      { x: 52, z: -150 }
    ],
    // 6) 테니스 ➔ 검술 남쪽 해안선 곡선 대로 (Southern Coastal Curve)
    [
      { x: -105, z: 55 },
      { x: -75, z: 105 },
      { x: -20, z: 145 },
      { x: 20, z: 158 },
      { x: 52, z: 150 }
    ]
  ];

  routesData.forEach((routePts) => {
    const { routeSampled } = buildRibbonMesh(routePts, 13.0);

    const lampSpacing = 28.0;
    const treeSpacing = 34.0;

    let lastLampDist = 8.0;
    let lastTreeDist = 12.0;

    let accumDist = 0;
    for (let i = 0; i < routeSampled.length; i++) {
      if (i > 0) {
        accumDist += routeSampled[i].pt.distanceTo(routeSampled[i - 1].pt);
      }

      const item = routeSampled[i];
      const halfW = 6.5;

      // 가로등 배치 (좌우 교차 배치)
      if (accumDist - lastLampDist >= lampSpacing) {
        lastLampDist = accumDist;
        const side = (streetLamps.length % 2 === 0) ? 1 : -1;
        const lampX = item.pt.x + item.norm.x * (halfW + 1.4) * side;
        const lampZ = item.pt.z + item.norm.z * (halfW + 1.4) * side;
        const rotY = Math.atan2(-item.norm.x * side, -item.norm.z * side);
        const hasLight = (streetLamps.length % 2 === 0);
        addStreetLamp(lampX, lampZ, rotY, hasLight);
      }

      // 길 주변 가로수 야자수 위치 등록 ("길에는 나무가 없고 길주변에는 나무가 있어")
      if (accumDist - lastTreeDist >= treeSpacing) {
        lastTreeDist = accumDist;
        // 길 왼쪽 야자수
        roadsideTreePositions.push(new THREE.Vector3(
          item.pt.x + item.norm.x * (halfW + 4.2 + Math.random() * 1.5),
          0,
          item.pt.z + item.norm.z * (halfW + 4.2 + Math.random() * 1.5)
        ));
        // 길 오른쪽 야자수
        roadsideTreePositions.push(new THREE.Vector3(
          item.pt.x - item.norm.x * (halfW + 4.2 + Math.random() * 1.5),
          0,
          item.pt.z - item.norm.z * (halfW + 4.2 + Math.random() * 1.5)
        ));
      }
    }
  });

  // 5. 도로 충돌 검사 함수
  function isPointOnRoad(x, z, margin = 6.0) {
    // 중앙 로터리 검사
    if (Math.hypot(x, z) < 25.5 + margin) return true;

    // 경로 샘플 포인트들과의 최단 거리 검사
    const threshold = 6.5 + margin;
    const threshSq = threshold * threshold;
    for (let i = 0; i < sampledPathPoints.length; i++) {
      const p = sampledPathPoints[i];
      const dx = x - p.x;
      const dz = z - p.z;
      if (dx * dx + dz * dz < threshSq) {
        return true;
      }
    }
    return false;
  }

  return {
    group: pathwaysGroup,
    streetLamps,
    lampBulbMaterial: lampBulbMat,
    roadsideTreePositions,
    isPointOnRoad
  };
}
