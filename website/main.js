import { TennisIslandCinematicViewer, ModelInspectModalViewer } from './viewer3d.js';

function initApp() {
  let islandViewer = null;
  let inspectViewer = null;

  // 1. Initialize Background Island 3D Viewer
  const islandCanvas = document.getElementById('island-canvas-container');
  if (islandCanvas) {
    try {
      islandViewer = new TennisIslandCinematicViewer('island-canvas-container');
    } catch (e) {
      console.error('Failed to init TennisIslandCinematicViewer:', e);
    }
  }

  // 2. Initialize Model Inspect Modal Viewer
  const inspectCanvas = document.getElementById('inspect-canvas-container');
  if (inspectCanvas) {
    try {
      inspectViewer = new ModelInspectModalViewer('inspect-canvas-container');
    } catch (e) {
      console.error('Failed to init ModelInspectModalViewer:', e);
    }
  }

  // Web Audio for bubbly popping feedback
  const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
  let soundEnabled = true;

  function playSound(type = 'pop') {
    if (!soundEnabled || !audioCtx) return;
    if (audioCtx.state === 'suspended') {
      audioCtx.resume();
    }
    const osc = audioCtx.createOscillator();
    const gain = audioCtx.createGain();
    osc.connect(gain);
    gain.connect(audioCtx.destination);

    const now = audioCtx.currentTime;
    if (type === 'whoosh') {
      osc.type = 'sine';
      osc.frequency.setValueAtTime(320, now);
      osc.frequency.exponentialRampToValueAtTime(680, now + 0.12);
      gain.gain.setValueAtTime(0.2, now);
      gain.gain.exponentialRampToValueAtTime(0.01, now + 0.12);
      osc.start(now);
      osc.stop(now + 0.12);
    } else {
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(540, now);
      osc.frequency.exponentialRampToValueAtTime(220, now + 0.08);
      gain.gain.setValueAtTime(0.15, now);
      gain.gain.exponentialRampToValueAtTime(0.01, now + 0.08);
      osc.start(now);
      osc.stop(now + 0.08);
    }
  }

  // =======================================================
  // Background Music (BGM): assets/music/메인테마곡.mp3 연결
  // =======================================================
  const bgm = new Audio('assets/music/메인테마곡.mp3');
  bgm.loop = true;
  bgm.volume = 0.5;
  let isMusicPlaying = false;

  const soundBtn = document.getElementById('btn-sound-toggle');

  function startBgm() {
    if (isMusicPlaying) return;
    bgm.play()
      .then(() => {
        isMusicPlaying = true;
        if (soundBtn) soundBtn.innerHTML = '🔊 ON';
      })
      .catch((e) => {
        console.log('Autoplay deferred until user interaction:', e);
      });
  }

  function toggleMusic() {
    if (isMusicPlaying) {
      bgm.pause();
      isMusicPlaying = false;
      if (soundBtn) soundBtn.innerHTML = '🔇 OFF';
      soundEnabled = false;
    } else {
      bgm.play().then(() => {
        isMusicPlaying = true;
        if (soundBtn) soundBtn.innerHTML = '🔊 ON';
        soundEnabled = true;
      });
    }
  }

  soundBtn?.addEventListener('click', () => {
    toggleMusic();
    playSound('pop');
  });

  // 사용자의 첫 인터랙션(클릭, 키 입력 등) 시 BGM 자연스러운 시작 시도
  const handleFirstInteraction = () => {
    startBgm();
    window.removeEventListener('click', handleFirstInteraction);
    window.removeEventListener('wheel', handleFirstInteraction);
    window.removeEventListener('keydown', handleFirstInteraction);
  };
  window.addEventListener('click', handleFirstInteraction, { once: true });
  window.addEventListener('wheel', handleFirstInteraction, { once: true });
  window.addEventListener('keydown', handleFirstInteraction, { once: true });

  // =======================================================
  // Toast Message Cards: 살짝만 굴려도 슉슉 반응하는 스냅 스크롤 제어
  // =======================================================
  const progressBar = document.getElementById('scroll-progress-indicator');
  const skyHint = document.getElementById('sky-hint-pill');

  let currentActiveIndex = 0;
  const TOTAL_STEPS = 4; // 0, 1, 2, 3
  let isTransitioning = false;
  let targetProgress = 0;
  let currentAnimatedProgress = 0;

  // Step별 카메라 시네마틱 위치 매핑 (0.0: 하늘 조망, 0.35: 코트 다이브, 0.7: 모션 뷰, 1.0: 3D 쇼케이스)
  const stepProgressMap = [0.0, 0.32, 0.68, 1.0];

  let enterCourtTimer = null;

  function goToStep(newIndex, isForward = true) {
    if (newIndex < 0 || newIndex >= TOTAL_STEPS) return;

    if (newIndex === currentActiveIndex) return;

    clearTimeout(enterCourtTimer);

    const oldCard = document.querySelector(`.floating-story-card[data-step="${currentActiveIndex}"]`);
    const newCard = document.querySelector(`.floating-story-card[data-step="${newIndex}"]`);

    if (oldCard) {
      oldCard.classList.remove('active', 'slide-out-up', 'slide-out-down');
      oldCard.classList.add(isForward ? 'slide-out-up' : 'slide-out-down');
    }

    // 첫 화면(0: 하늘)에서 1번(경기장 코트)으로 이동할 때만:
    // 카메라가 경기장으로 슈웅 이동한 뒤 1초 후에 카드가 스르륵 뜨도록 연출
    if (newIndex === 1 && currentActiveIndex === 0) {
      if (newCard) {
        newCard.classList.remove('active', 'slide-out-up', 'slide-out-down');
      }
      enterCourtTimer = setTimeout(() => {
        if (currentActiveIndex === 1 && newCard) {
          newCard.classList.remove('slide-out-up', 'slide-out-down');
          newCard.classList.add('active');
          playSound('whoosh');
        }
      }, 1000);
    } else {
      if (newCard) {
        newCard.classList.remove('slide-out-up', 'slide-out-down');
        newCard.classList.add('active');
      }
      playSound('whoosh');
    }

    currentActiveIndex = newIndex;
    targetProgress = stepProgressMap[newIndex] || 0;

    // 프로그레스 바 갱신
    if (progressBar) {
      progressBar.style.width = newIndex === 0 ? '0%' : `${(newIndex / (TOTAL_STEPS - 1)) * 100}%`;
    }

    // 첫 스텝 벗어나면 하늘 힌트 숨기기
    if (skyHint) {
      if (newIndex > 0) skyHint.classList.add('hidden');
      else skyHint.classList.remove('hidden');
    }
  }

  // 휠을 살짝만 돌려도 즉각 즉각 전환되는 고감도 인터셉터
  let wheelTimeout = null;
  window.addEventListener(
    'wheel',
    (e) => {
      // 3D 검사 모달이 열려있을 때는 모달 내부 줌/조작을 위해 페이지 전환 스킵
      if (inspectModal && inspectModal.classList.contains('active')) return;

      e.preventDefault();

      if (isTransitioning) return;

      const threshold = 12; // 살짝만 움직여도 반응
      if (Math.abs(e.deltaY) < threshold) return;

      isTransitioning = true;
      if (e.deltaY > 0) {
        // 휠 내림 -> 다음 스텝
        goToStep(currentActiveIndex + 1, true);
      } else {
        // 휠 올림 -> 이전 스텝
        goToStep(currentActiveIndex - 1, false);
      }

      // 380ms 쿨다운 후 다음 휠 입력 허용 (부드러운 전환감 보장)
      clearTimeout(wheelTimeout);
      wheelTimeout = setTimeout(() => {
        isTransitioning = false;
      }, 380);
    },
    { passive: false }
  );

  // 키보드 방향키 제어
  window.addEventListener('keydown', (e) => {
    if (inspectModal && inspectModal.classList.contains('active')) return;
    if (['ArrowDown', 'PageDown', ' '].includes(e.key)) {
      e.preventDefault();
      goToStep(currentActiveIndex + 1, true);
    } else if (['ArrowUp', 'PageUp'].includes(e.key)) {
      e.preventDefault();
      goToStep(currentActiveIndex - 1, false);
    }
  });

  // 모바일 터치 스와이프 제어
  let touchStartY = 0;
  window.addEventListener(
    'touchstart',
    (e) => {
      if (e.touches.length > 0) touchStartY = e.touches[0].clientY;
    },
    { passive: true }
  );

  window.addEventListener(
    'touchend',
    (e) => {
      if (inspectModal && inspectModal.classList.contains('active')) return;
      if (e.changedTouches.length > 0) {
        const touchEndY = e.changedTouches[0].clientY;
        const diffY = touchStartY - touchEndY;
        if (Math.abs(diffY) > 30) {
          if (diffY > 0) goToStep(currentActiveIndex + 1, true);
          else goToStep(currentActiveIndex - 1, false);
        }
      }
    },
    { passive: true }
  );

  // 카메라 프로그레스 부드러운 애니메이션 루프
  function animateCameraStep() {
    requestAnimationFrame(animateCameraStep);
    const diff = targetProgress - currentAnimatedProgress;
    if (Math.abs(diff) > 0.001) {
      currentAnimatedProgress += diff * 0.08;
      if (islandViewer) {
        islandViewer.updateCameraForScroll(currentAnimatedProgress);
      }
    }
  }
  requestAnimationFrame(animateCameraStep);

  // =======================================================
  // 3D Model Inspect Modal Window ("슉 생기는 파트")
  // =======================================================
  const inspectModal = document.getElementById('model-inspect-modal');
  const btnCloseInspect = document.getElementById('btn-close-inspect');
  const btnOpenModal = document.getElementById('btn-open-3d-modal');
  const btnTriggerViewer = document.getElementById('btn-trigger-3d-viewer');
  const btnReInspect = document.getElementById('btn-re-inspect');

  function openInspectModal(initialModel = 'character') {
    if (!inspectModal) return;
    inspectModal.classList.add('active');
    playSound('whoosh');

    if (inspectViewer) {
      setTimeout(() => {
        inspectViewer.resize();
        inspectViewer.loadModel(initialModel);
        updateInspectTabs(initialModel);
      }, 50);
    }
  }

  function closeInspectModal() {
    if (!inspectModal) return;
    inspectModal.classList.remove('active');
    playSound('pop');
  }

  btnOpenModal?.addEventListener('click', () => openInspectModal('character'));
  btnTriggerViewer?.addEventListener('click', () => openInspectModal('character'));
  btnReInspect?.addEventListener('click', () => openInspectModal('character'));
  btnCloseInspect?.addEventListener('click', closeInspectModal);

  inspectModal?.addEventListener('click', (e) => {
    if (e.target === inspectModal) closeInspectModal();
  });

  // Modal Model Tabs (Character, Racket 6-colors, Ball, Court)
  const tabBtns = document.querySelectorAll('.inspect-tab-btn');
  const characterMotionRow = document.getElementById('character-motion-row');
  const racketColorRow = document.getElementById('racket-color-row');
  const modelTipText = document.getElementById('inspect-model-tip');

  const tipsMap = {
    character: '마우스 드래그로 360° 회전하고, 아래 모션 버튼으로 5가지 동작을 감상하세요!',
    racket: '아래 6가지 컬러 버튼을 눌러 라켓의 컬러와 텍스처를 실시간으로 바꿔보세요!',
    ball: '공인구 텍스처와 노멀맵이 적용된 테니스 볼을 마우스 드래그로 자유롭게 둘러보세요!',
    court: '마우스 드래그로 챔피언십 테니스 코트 라인과 네트를 둘러보세요!'
  };

  function updateInspectTabs(type) {
    tabBtns.forEach((b) => b.classList.toggle('active', b.getAttribute('data-model') === type));
    if (characterMotionRow) {
      characterMotionRow.style.display = type === 'character' ? 'flex' : 'none';
    }
    if (racketColorRow) {
      racketColorRow.style.display = type === 'racket' ? 'flex' : 'none';
    }
    if (modelTipText && tipsMap[type]) {
      modelTipText.innerText = tipsMap[type];
    }
  }

  tabBtns.forEach((btn) => {
    btn.addEventListener('click', () => {
      const model = btn.getAttribute('data-model');
      if (inspectViewer && model) {
        playSound('pop');
        inspectViewer.loadModel(model);
        updateInspectTabs(model);
      }
    });
  });

  // Character Motion Buttons (5개 모션 실시간 전환)
  const motionBtns = document.querySelectorAll('.motion-btn');
  motionBtns.forEach((btn) => {
    btn.addEventListener('click', () => {
      const motion = btn.getAttribute('data-motion');
      if (inspectViewer && motion) {
        motionBtns.forEach((b) => b.classList.remove('active'));
        btn.classList.add('active');
        playSound('pop');
        inspectViewer.loadCharacterMotion(motion);
      }
    });
  });

  // 6 Racket Color Buttons (텍스처 실시간 교체)
  const paletteBtns = document.querySelectorAll('.palette-btn');
  paletteBtns.forEach((btn) => {
    btn.addEventListener('click', () => {
      const color = btn.getAttribute('data-color');
      if (inspectViewer && color) {
        paletteBtns.forEach((b) => b.classList.remove('active'));
        btn.classList.add('active');
        playSound('pop');
        inspectViewer.setRacketColor(color);
      }
    });
  });
}

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initApp);
} else {
  initApp();
}
