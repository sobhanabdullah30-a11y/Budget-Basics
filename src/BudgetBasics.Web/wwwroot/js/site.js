(() => {
  "use strict";

  const token = document.querySelector('#security-token input[name="__RequestVerificationToken"]')?.value;
  const money = value => new Intl.NumberFormat('en-PK', { style: 'currency', currency: 'PKR', maximumFractionDigits: 0 }).format(value);
  const escapeHtml = value => String(value).replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[character]);
  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  document.documentElement.classList.add('motion-ready');

  const siteHeader = document.querySelector('.site-header');
  const progressBar = document.getElementById('scroll-progress-bar');
  let scrollFrame = 0;
  const updatePagePosition = () => {
    const scrollable = Math.max(document.documentElement.scrollHeight - window.innerHeight, 1);
    const progress = Math.min(Math.max(window.scrollY / scrollable, 0), 1);
    if (progressBar) progressBar.style.transform = `scaleX(${progress})`;
    siteHeader?.classList.toggle('scrolled', window.scrollY > 20);
    scrollFrame = 0;
  };
  window.addEventListener('scroll', () => {
    if (!scrollFrame) scrollFrame = requestAnimationFrame(updatePagePosition);
  }, { passive: true });
  updatePagePosition();

  const revealTargets = [...document.querySelectorAll('.section-heading,.concept-card,.sample-budget,.needs-panel,.tool-card,.planner,.mistake-grid details,.smart-tip-grid>div,.smart-tip-card,.gallery figure,.assistant-grid>div,.contact-grid>div,.feedback-card,.auth-card,.footer-grid>div')];
  if ('IntersectionObserver' in window && !reducedMotion) {
    revealTargets.forEach((item, index) => {
      item.classList.add('reveal-item');
      item.style.setProperty('--reveal-delay', `${(index % 4) * 70}ms`);
    });
    const revealObserver = new IntersectionObserver(entries => {
      entries.forEach(entry => {
        if (!entry.isIntersecting) return;
        entry.target.classList.add('is-visible');
        revealObserver.unobserve(entry.target);
      });
    }, { rootMargin: '0px 0px -8% 0px', threshold: 0.08 });
    revealTargets.forEach(item => revealObserver.observe(item));
  }

  if (!reducedMotion) {
    document.querySelectorAll('[data-tilt]').forEach(card => {
      card.addEventListener('pointermove', event => {
        if (event.pointerType === 'touch') return;
        const bounds = card.getBoundingClientRect();
        const x = (event.clientX - bounds.left) / bounds.width;
        const y = (event.clientY - bounds.top) / bounds.height;
        card.style.setProperty('--tilt-x', `${(0.5 - y) * 8}deg`);
        card.style.setProperty('--tilt-y', `${(x - 0.5) * 10}deg`);
        card.style.setProperty('--pointer-x', `${x * 100}%`);
        card.style.setProperty('--pointer-y', `${y * 100}%`);
      });
      card.addEventListener('pointerleave', () => {
        card.style.setProperty('--tilt-x', '0deg');
        card.style.setProperty('--tilt-y', '0deg');
        card.style.setProperty('--pointer-x', '50%');
        card.style.setProperty('--pointer-y', '50%');
      });
    });

    document.querySelectorAll('[data-counter]').forEach(counter => {
      const target = Number(counter.dataset.counter);
      if (!Number.isFinite(target)) return;
      const duration = 900;
      const started = performance.now();
      const count = now => {
        const progress = Math.min((now - started) / duration, 1);
        const eased = 1 - Math.pow(1 - progress, 3);
        counter.textContent = Math.round(target * eased).toLocaleString();
        if (progress < 1) requestAnimationFrame(count);
      };
      requestAnimationFrame(count);
    });
  }

  document.querySelectorAll('.btn').forEach(button => button.addEventListener('pointerdown', event => {
    if (reducedMotion) return;
    const bounds = button.getBoundingClientRect();
    const ripple = document.createElement('span');
    const size = Math.max(bounds.width, bounds.height);
    ripple.className = 'ripple';
    ripple.style.width = ripple.style.height = `${size}px`;
    ripple.style.left = `${event.clientX - bounds.left - size / 2}px`;
    ripple.style.top = `${event.clientY - bounds.top - size / 2}px`;
    button.appendChild(ripple);
    ripple.addEventListener('animationend', () => ripple.remove(), { once: true });
  }));

  async function postForm(url, formData) {
    let response;
    try {
      response = await fetch(url, {
        method: 'POST',
        credentials: 'same-origin',
        headers: { Accept: 'application/json', ...(token ? { RequestVerificationToken: token } : {}) },
        body: formData
      });
    } catch {
      throw new Error('The server could not be reached. Check your connection and try again.');
    }

    const text = response.status === 204 ? '' : await response.text();
    let data = null;
    if (text) {
      try { data = JSON.parse(text); }
      catch { data = null; }
    }
    if (!response.ok) {
      if (response.status === 400 && !data?.message) {
        throw new Error('This form expired after a server restart. Refresh the page once, then try again.');
      }
      throw new Error(data?.message || `The request could not be completed (error ${response.status}). Please try again.`);
    }
    if (response.status !== 204 && !data) throw new Error('The server returned an incomplete response. Please try again.');
    return data;
  }

  const themeToggle = document.getElementById('theme-toggle');
  const savedTheme = localStorage.getItem('bb-theme');
  if (savedTheme) document.documentElement.dataset.theme = savedTheme;
  const updateThemeControl = () => {
    if (!themeToggle) return;
    const dark = document.documentElement.dataset.theme === 'dark';
    themeToggle.setAttribute('aria-pressed', String(dark));
    themeToggle.setAttribute('aria-label', dark ? 'Switch to light theme' : 'Switch to dark theme');
  };
  updateThemeControl();
  themeToggle?.addEventListener('click', () => {
    const next = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
    document.documentElement.dataset.theme = next;
    localStorage.setItem('bb-theme', next);
    updateThemeControl();
  });

  const sectionLinks = [...document.querySelectorAll('.nav-link[data-section]')];
  const setActiveSection = id => sectionLinks.forEach(link => {
    const active = link.dataset.section === id;
    link.classList.toggle('active', active);
    if (active) link.setAttribute('aria-current', 'location'); else link.removeAttribute('aria-current');
  });
  if (sectionLinks.length && 'IntersectionObserver' in window) {
    const observer = new IntersectionObserver(entries => {
      const visible = entries.filter(entry => entry.isIntersecting).sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];
      if (visible) setActiveSection(visible.target.id);
    }, { rootMargin: '-25% 0px -60% 0px', threshold: [0.05, 0.2, 0.5] });
    sectionLinks.forEach(link => {
      const section = document.getElementById(link.dataset.section);
      if (section) observer.observe(section);
    });
  }

  const clock = document.getElementById('live-clock');
  const updateClock = () => {
    if (clock) clock.textContent = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'medium' }).format(new Date());
  };
  updateClock();
  setInterval(updateClock, 1000);

  const quizQuestions = [
    { text: "A textbook is required for tomorrow's class. Is it a need or a want?", answer: 'need', explanation: 'Required course material supports an immediate education need.' },
    { text: 'You want premium takeaway even though a meal is ready at home. Is it a need or a want?', answer: 'want', explanation: 'The extra takeaway is optional because an adequate meal is already available.' },
    { text: 'You need prescribed medicine for an illness. Is it a need or a want?', answer: 'need', explanation: 'Prescribed medicine supports health and is an essential need.' },
    { text: 'You want to upgrade a working streaming plan for more channels. Is it a need or a want?', answer: 'want', explanation: 'An entertainment upgrade is optional and can wait.' },
    { text: 'You need safe transport to attend an exam. Is it a need or a want?', answer: 'need', explanation: 'Safe transport for an essential commitment is a need.' },
    { text: 'You want the newest phone while your current phone works properly. Is it a need or a want?', answer: 'want', explanation: 'A newer model is an optional upgrade when the current device still works.' }
  ];
  let quizIndex = 0;
  let quizScore = 0;
  const quizChoices = [...document.querySelectorAll('[data-answer]')];
  const quizQuestion = document.getElementById('quiz-question');
  const quizProgress = document.getElementById('quiz-progress');
  const quizFeedback = document.getElementById('quiz-feedback');
  const nextQuiz = document.getElementById('next-quiz');
  const renderQuizQuestion = () => {
    const question = quizQuestions[quizIndex];
    if (!question || !quizQuestion) return;
    quizQuestion.textContent = question.text;
    quizProgress.textContent = `Question ${quizIndex + 1} of ${quizQuestions.length}`;
    quizFeedback.textContent = '';
    quizChoices.forEach(choice => {
      choice.disabled = false;
      choice.classList.remove('is-correct', 'is-wrong');
    });
    nextQuiz.hidden = true;
  };
  quizChoices.forEach(button => button.addEventListener('click', () => {
    const question = quizQuestions[quizIndex];
    const correct = button.dataset.answer === question.answer;
    quizChoices.forEach(choice => {
      choice.disabled = true;
      choice.classList.toggle('is-correct', choice.dataset.answer === question.answer);
    });
    if (!correct) button.classList.add('is-wrong');
    else quizScore += 1;
    quizFeedback.textContent = `${correct ? 'Correct!' : 'Not this time.'} ${question.explanation}`;
    quizFeedback.style.color = correct ? 'var(--green)' : '#a15b17';
    nextQuiz.textContent = quizIndex === quizQuestions.length - 1 ? 'See my result →' : 'Next question →';
    nextQuiz.hidden = false;
  }));
  nextQuiz?.addEventListener('click', () => {
    if (quizIndex === quizQuestions.length - 1) {
      quizFeedback.textContent = `You scored ${quizScore} out of ${quizQuestions.length}. ${quizScore >= 5 ? 'Excellent work!' : 'Review the examples and try again.'}`;
      quizFeedback.style.color = 'var(--green)';
      nextQuiz.textContent = 'Try again';
      quizIndex = -1;
      quizScore = 0;
      quizChoices.forEach(choice => choice.classList.remove('is-correct', 'is-wrong'));
      return;
    }
    quizIndex += 1;
    renderQuizQuestion();
  });

  document.getElementById('budget-form')?.addEventListener('submit', async event => {
    event.preventDefault();
    const form = event.currentTarget;
    const result = document.getElementById('budget-result');
    if (!form.reportValidity()) return;
    try {
      const data = await postForm('/tools/budget-split', new FormData(form));
      result.innerHTML = `<strong>Suggested monthly split</strong><div class="result-row"><span>Needs · 50%</span><b>${money(data.needs)}</b></div><div class="result-row"><span>Wants · 30%</span><b>${money(data.wants)}</b></div><div class="result-row"><span>Savings · 20%</span><b>${money(data.savings)}</b></div>`;
    } catch (error) { result.textContent = error.message; }
    result.hidden = false;
  });

  document.getElementById('goal-form')?.addEventListener('submit', async event => {
    event.preventDefault();
    const form = event.currentTarget;
    const result = document.getElementById('goal-result');
    if (!form.reportValidity()) return;
    try {
      const data = await postForm('/tools/savings-goal', new FormData(form));
      result.innerHTML = `<strong>${escapeHtml(data.goalName)}</strong><div class="result-progress"><i style="width:${data.progressPercent}%"></i></div><div class="result-row"><span>Progress</span><b>${data.progressPercent}%</b></div><div class="result-row"><span>Remaining</span><b>${money(data.remainingAmount)}</b></div><div class="result-row"><span>Estimated time</span><b>${data.estimatedMonths} month${data.estimatedMonths === 1 ? '' : 's'}</b></div><small>Keep going — regular contributions make the goal easier to manage.</small>`;
    } catch (error) { result.textContent = error.message; }
    result.hidden = false;
  });

  let expenses = [];
  const expenseForm = document.getElementById('expense-form');
  const planner = document.querySelector('.planner[data-persistent]');
  const persistExpenses = planner?.dataset.persistent === 'true';
  const expenseStatus = document.getElementById('expense-status');
  const setExpenseStatus = message => { if (expenseStatus) expenseStatus.textContent = message; };
  const renderExpenses = () => {
    const list = document.getElementById('expense-list');
    if (!list) return;
    if (!expenses.length) list.innerHTML = '<tr class="empty-row"><td colspan="5">No expenses yet.</td></tr>';
    else list.innerHTML = expenses.map(item => `<tr><td>${escapeHtml(item.date)}</td><td>${escapeHtml(item.category)}</td><td>${escapeHtml(item.description)}</td><td>${money(item.amount)}</td><td><button class="table-action" type="button" data-edit="${item.id}">Edit</button><button class="table-action delete" type="button" data-delete="${item.id}">Remove</button></td></tr>`).join('');
    const total = expenses.reduce((sum, item) => sum + item.amount, 0);
    const balance = Number(document.getElementById('planner-balance')?.value || 0);
    document.getElementById('expense-total').textContent = money(total);
    document.getElementById('remaining-balance').textContent = money(balance - total);
  };
  expenseForm?.addEventListener('submit', async event => {
    event.preventDefault();
    if (!expenseForm.reportValidity()) return;
    const idField = document.getElementById('expense-id');
    let item = { id: idField.value || crypto.randomUUID(), date: document.getElementById('expense-date').value, category: document.getElementById('expense-category').value, description: document.getElementById('expense-description').value.trim(), amount: Number(document.getElementById('expense-amount').value) };
    try {
      if (persistExpenses) {
        item = await postForm('/tools/expenses/save', new FormData(expenseForm));
        setExpenseStatus('Expense saved to your account.');
      }
      const existingIndex = expenses.findIndex(entry => String(entry.id) === String(item.id));
      if (existingIndex >= 0) expenses[existingIndex] = item; else expenses.push(item);
    } catch (error) {
      setExpenseStatus(error.message);
      return;
    }
    expenseForm.reset(); idField.value = ''; document.getElementById('expense-date').valueAsDate = new Date(); renderExpenses();
  });
  document.getElementById('expense-list')?.addEventListener('click', async event => {
    const editId = event.target.dataset.edit; const deleteId = event.target.dataset.delete;
    if (deleteId) {
      try {
        if (persistExpenses) {
          const data = new FormData(); data.set('Id', deleteId);
          await postForm('/tools/expenses/delete', data);
          setExpenseStatus('Expense removed from your account.');
        }
        expenses.splice(expenses.findIndex(item => String(item.id) === deleteId), 1);
      } catch (error) { setExpenseStatus(error.message); return; }
    }
    if (editId) { const item = expenses.find(entry => String(entry.id) === editId); document.getElementById('expense-id').value = item.id; document.getElementById('expense-date').value = item.date; document.getElementById('expense-category').value = item.category; document.getElementById('expense-description').value = item.description; document.getElementById('expense-amount').value = item.amount; }
    renderExpenses();
  });
  document.getElementById('planner-balance')?.addEventListener('input', renderExpenses);
  const dateInput = document.getElementById('expense-date'); if (dateInput) dateInput.valueAsDate = new Date();
  if (persistExpenses) fetch('/tools/expenses', { headers: { Accept: 'application/json' } })
    .then(response => response.ok ? response.json() : Promise.reject(new Error('Please sign in again to load saved expenses.')))
    .then(data => { expenses = data; renderExpenses(); setExpenseStatus(`${data.length} saved expense${data.length === 1 ? '' : 's'} loaded.`); })
    .catch(error => setExpenseStatus(error.message));

  const applyContentFilter = () => {
    const query = (document.getElementById('content-search')?.value || '').trim().toLowerCase();
    const selected = document.querySelector('.filter-row button.active')?.dataset.filter || 'all';
    let galleryMatches = 0;
    document.querySelectorAll('[data-topic]').forEach(item => {
      const topics = item.dataset.topic.toLowerCase();
      const matches = (!query || topics.includes(query) || item.textContent.toLowerCase().includes(query)) && (selected === 'all' || topics.includes(selected));
      item.hidden = !matches;
      if (matches && item.closest('.gallery')) galleryMatches++;
    });
    const noResults = document.getElementById('no-results'); if (noResults) noResults.hidden = galleryMatches > 0;
  };
  document.getElementById('content-search')?.addEventListener('input', applyContentFilter);
  document.querySelectorAll('.filter-row button').forEach(button => button.addEventListener('click', () => { document.querySelectorAll('.filter-row button').forEach(item => item.classList.remove('active')); button.classList.add('active'); applyContentFilter(); }));
  const gallery = document.querySelector('.gallery');
  const initialGalleryOrder = gallery ? [...gallery.querySelectorAll('figure')] : [];
  document.getElementById('content-sort')?.addEventListener('change', event => {
    const figures = [...initialGalleryOrder];
    if (event.target.value === 'az') figures.sort((a, b) => a.querySelector('figcaption b').textContent.localeCompare(b.querySelector('figcaption b').textContent));
    figures.forEach(figure => gallery.appendChild(figure));
  });

  const addChatMessage = (messages, text, className, topic) => {
    const message = document.createElement('div');
    message.className = className;
    if (topic) {
      const label = document.createElement('small');
      label.className = 'chat-topic';
      label.textContent = topic;
      message.appendChild(label);
    }
    message.appendChild(document.createTextNode(text));
    messages.appendChild(message);
  };
  document.querySelectorAll('.prompt-list button').forEach(button => button.addEventListener('click', () => {
    const question = document.getElementById('chat-question');
    question.value = button.textContent;
    document.getElementById('chat-form').requestSubmit();
  }));
  document.getElementById('chat-form')?.addEventListener('submit', async event => {
    event.preventDefault();
    const form = event.currentTarget;
    if (!form.reportValidity()) return;
    const messages = document.getElementById('chat-messages');
    const question = document.getElementById('chat-question');
    const submit = form.querySelector('button[type="submit"]');
    const questionText = question.value.trim();
    const request = new FormData();
    request.append('Question', questionText);
    addChatMessage(messages, questionText, 'user-message');
    form.reset();
    submit.disabled = true;
    question.disabled = true;
    messages.setAttribute('aria-busy', 'true');
    const typing = document.createElement('div');
    typing.className = 'bot-message typing-message';
    typing.textContent = 'BudgetBee is thinking…';
    messages.appendChild(typing);
    messages.scrollTop = messages.scrollHeight;
    try {
      const data = await postForm('/tools/chat', request);
      typing.remove();
      addChatMessage(messages, data.answer, 'bot-message', data.topic);
    } catch (error) {
      typing.remove();
      addChatMessage(messages, error.message, 'bot-message');
    } finally {
      submit.disabled = false;
      question.disabled = false;
      messages.setAttribute('aria-busy', 'false');
      question.focus();
      messages.scrollTop = messages.scrollHeight;
    }
  });
  document.getElementById('clear-chat')?.addEventListener('click', () => {
    const messages = document.getElementById('chat-messages');
    messages.replaceChildren();
    addChatMessage(messages, 'Conversation cleared. What would you like to learn about?', 'bot-message');
    document.getElementById('chat-question').focus();
  });

  document.getElementById('habit-tip-button')?.addEventListener('click', async event => {
    const button = event.currentTarget;
    const habit = document.getElementById('habit-tip-select')?.value || 'all';
    const result = document.getElementById('habit-tip-result');
    button.disabled = true;
    result.setAttribute('aria-busy', 'true');
    result.querySelector('h3').textContent = 'Finding a useful tip…';
    result.querySelector('p').textContent = 'Checking the BudgetBasics learning library.';
    try {
      const response = await fetch(`/tools/student-tip?habit=${encodeURIComponent(habit)}`, { headers: { 'Accept': 'application/json' } });
      const data = await response.json();
      if (!response.ok) throw new Error(data.message || 'The tip could not be loaded.');
      result.querySelector('h3').textContent = data.title;
      result.querySelector('p').textContent = data.message;
      result.classList.remove('tip-refresh');
      requestAnimationFrame(() => result.classList.add('tip-refresh'));
    } catch (error) {
      result.querySelector('h3').textContent = 'Tip temporarily unavailable';
      result.querySelector('p').textContent = error.message;
    } finally {
      button.disabled = false;
      result.setAttribute('aria-busy', 'false');
    }
  });

  document.getElementById('feedback-form')?.addEventListener('submit', async event => {
    event.preventDefault(); const form = event.currentTarget; if (!form.reportValidity()) return;
    const message = document.getElementById('feedback-message');
    try { const data = await postForm('/tools/feedback', new FormData(form)); message.textContent = data.message; form.reset(); }
    catch (error) { message.textContent = error.message; }
  });

  document.getElementById('contact-form')?.addEventListener('submit', event => {
    event.preventDefault();
    const form = event.currentTarget;
    if (!form.reportValidity()) return;
    const message = document.getElementById('contact-message');
    message.textContent = 'Thank you. Your message passed validation. This educational demo does not transmit or store contact messages; use the feedback form for a saved submission.';
    form.reset();
  });

  if (token && document.body.textContent.includes('My account')) setInterval(() => postForm('/Account/Refresh', new FormData()).catch(() => {}), 10 * 60 * 1000);
})();
