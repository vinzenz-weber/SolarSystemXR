/**
 * Erstellt Google Forms fuer das User Testing von "Solar System XR".
 *
 * Verwendung:
 * 1. In Google Drive ein neues Apps-Script-Projekt erstellen.
 * 2. Diesen kompletten Code in die Datei Code.gs kopieren.
 * 3. Die Funktion createSolarSystemXRUserTestingForms ausfuehren.
 * 4. Die erzeugten Form-Links im Ausfuehrungsprotokoll kopieren.
 */

function createSolarSystemXRUserTestingForms() {
  const participantForm = createParticipantForm();
  const observationForm = createObservationForm();

  Logger.log('Participant form edit URL: ' + participantForm.getEditUrl());
  Logger.log('Participant form public URL: ' + participantForm.getPublishedUrl());
  Logger.log('Observation form edit URL: ' + observationForm.getEditUrl());
  Logger.log('Observation form public URL: ' + observationForm.getPublishedUrl());
}

function createParticipantForm() {
  const form = FormApp.create('Solar System XR - User Testing Questionnaire');

  form.setDescription(
    'Participant-facing questionnaire for the Solar System XR user testing session.\n\n' +
      'The prototype is part of a master thesis and explores how immersive technologies can support learning about planets, distances, sizes, and orbital movement.'
  );
  form.setCollectEmail(false);
  form.setAllowResponseEdits(false);
  form.setConfirmationMessage('Thank you for taking part in this user test.');
  form.setIsQuiz(true);

  form
    .addSectionHeaderItem()
    .setTitle('Short Study Introduction')
    .setHelpText(
      'Thank you for taking part in this user test.\n\n' +
        'In this session, you will try a prototype of an XR application about the solar system. The application is part of a master thesis and explores how immersive technologies can support learning about planets, distances, sizes, and orbital movement.\n\n' +
        'This is not a test of your personal knowledge or ability. The goal is to evaluate the prototype: what is understandable, what supports learning, what is motivating, and what should be improved.\n\n' +
        'You can ask questions at any time. You can also pause or stop the test at any point without giving a reason.'
    );

  const consentItem = form
    .addMultipleChoiceItem()
    .setTitle('Consent')
    .setHelpText(
      'Please read the following statement:\n\n' +
        'I voluntarily agree to take part in this user test. I understand that I can stop participating at any time without giving a reason. The collected data will be anonymized and used only for the evaluation of the XR prototype "Solar System XR" as part of a master thesis.'
    )
    .setRequired(true);

  const preTestPage = form
    .addPageBreakItem()
    .setTitle('Pre-Test Questionnaire')
    .setHelpText('Please answer the following questions before using the XR prototype.');

  consentItem.setChoices([
    consentItem.createChoice('Yes, I agree.', preTestPage),
    consentItem.createChoice('No, I do not agree.', FormApp.PageNavigationType.SUBMIT),
  ]);

  addParticipantInformation(form);
  addKnowledgeQuestions(form, 'Pre-Test Knowledge Questions', 'Pre');
  addSelfAssessment(
    form,
    'Pre-Test Self-Assessment',
    [
      'I can name the planets in the correct order.',
      'I can imagine the size differences between the planets well.',
      'I can imagine the distances between the Sun and the planets well.',
      'I understand what an orbit is.',
      'I find the solar system interesting.',
    ],
    'Scale: 1 = strongly disagree, 5 = strongly agree.'
  );

  form
    .addPageBreakItem()
    .setTitle('XR Session Reflection')
    .setHelpText(
      'Please answer this directly after the XR session, before the post-test questionnaire.'
    );

  form
    .addParagraphTextItem()
    .setTitle(
      'What did you learn or understand better about the solar system that was less clear to you before?'
    )
    .setRequired(true);

  addTaskReflectionQuestions(form);

  form
    .addPageBreakItem()
    .setTitle('Post-Test Questionnaire')
    .setHelpText('Please answer the following questions after using the XR prototype.');

  addKnowledgeQuestions(form, 'Post-Test Knowledge Questions', 'Post');
  addSelfAssessment(
    form,
    'Post-Test Self-Assessment',
    [
      'I can name the planets in the correct order.',
      'I can imagine the size differences between the planets well.',
      'I can imagine the distances between the Sun and the planets well.',
      'I understand what an orbit is.',
      'I find the solar system interesting.',
    ],
    'Scale: 1 = strongly disagree, 5 = strongly agree.'
  );

  addSelfAssessment(
    form,
    'Value of XR Visualization',
    [
      'The spatial visualization helped me understand the solar system better.',
      'Being able to look around in space was useful for this topic.',
      'The XR visualization made sizes and distances clearer than a normal image would.',
      'The controls did not distract me from learning.',
      'I felt well oriented in the virtual space.',
    ],
    'Scale: 1 = strongly disagree, 5 = strongly agree.'
  );

  addSelfAssessment(
    form,
    'Gamification and Motivation',
    [
      'The minigames motivated me to engage with the planets.',
      'The direct feedback in the tasks helped me learn.',
      'The game-like tasks felt appropriate for the topic of the solar system.',
      'The tasks were challenging but not overwhelming.',
      'I would try more tasks or levels like this.',
    ],
    'Scale: 1 = strongly disagree, 5 = strongly agree.'
  );

  addSelfAssessment(
    form,
    'Scientific Credibility and Trust',
    [
      'The application felt scientifically credible.',
      'The information about the planets was easy to understand.',
      'The application made it clear that solar system visualizations need to be scaled or simplified.',
      'I had the impression that the planets were represented based on data, not randomly.',
      'I would like to see sources or additional information inside the application.',
    ],
    'Scale: 1 = strongly disagree, 5 = strongly agree.'
  );

  addSelfAssessment(
    form,
    'Usability',
    [
      'I was able to use the main functions of the app easily.',
      'The menus and buttons were easy to reach.',
      'The text was readable in XR.',
      'I usually knew what to do next.',
      'Interacting with planets and tasks felt understandable.',
      'I felt physically comfortable while using the application.',
    ],
    'Scale: 1 = strongly disagree, 5 = strongly agree.'
  );

  addOpenQuestions(form);

  return form;
}

function addParticipantInformation(form) {
  form
    .addSectionHeaderItem()
    .setTitle('Participant Information')
    .setHelpText('Basic participant information. Please do not enter names.');

  form
    .addTextItem()
    .setTitle('Participant ID')
    .setHelpText('For example: P01')
    .setRequired(true);

  form
    .addTextItem()
    .setTitle('Age')
    .setHelpText('Free text or age group')
    .setRequired(true);

  form
    .addParagraphTextItem()
    .setTitle('Study or professional background')
    .setRequired(false);

  addMultipleChoiceQuestion(form, 'Have you used VR/XR headsets before?', [
    'No',
    'Once or rarely',
    'Several times',
    'Regularly',
  ]);

  form
    .addScaleItem()
    .setTitle('How would you rate your knowledge about the solar system?')
    .setBounds(1, 5)
    .setLabels('very low', 'very good')
    .setRequired(true);

  form
    .addScaleItem()
    .setTitle('How confident are you in estimating astronomical sizes and distances?')
    .setBounds(1, 5)
    .setLabels('very unsure', 'very confident')
    .setRequired(true);
}

function addKnowledgeQuestions(form, sectionTitle, prefix) {
  form
    .addSectionHeaderItem()
    .setTitle(sectionTitle)
    .setHelpText('Each correct answer gives 1 point. Maximum score in this section: 10 points.');

  const questions = getKnowledgeQuestions();

  questions.forEach(function (question, index) {
    const item = form
      .addMultipleChoiceItem()
      .setTitle(prefix + ' Q' + (index + 1) + ': ' + question.title)
      .setRequired(true)
      .setPoints(1);

    item.setChoices(
      question.options.map(function (option) {
        return item.createChoice(option, option === question.correct);
      })
    );
  });
}

function getKnowledgeQuestions() {
  return [
    {
      title: 'How many planets are there in our solar system according to the current definition?',
      options: ['7', '8', '9', '10'],
      correct: '8',
    },
    {
      title: 'Which planet is closest to the Sun?',
      options: ['Venus', 'Mercury', 'Earth', 'Mars'],
      correct: 'Mercury',
    },
    {
      title: 'Which order of the four inner planets from the Sun outward is correct?',
      options: [
        'Mercury, Venus, Earth, Mars',
        'Venus, Mercury, Earth, Mars',
        'Mercury, Earth, Venus, Mars',
        'Earth, Venus, Mercury, Mars',
      ],
      correct: 'Mercury, Venus, Earth, Mars',
    },
    {
      title: 'Which planet is the largest planet in the solar system?',
      options: ['Earth', 'Saturn', 'Jupiter', 'Neptune'],
      correct: 'Jupiter',
    },
    {
      title: 'Which planet is the smallest of the eight planets?',
      options: ['Mars', 'Mercury', 'Venus', 'Neptune'],
      correct: 'Mercury',
    },
    {
      title: 'Which statement about Jupiter and Earth is most accurate?',
      options: [
        'Jupiter is about the same size as Earth.',
        'Jupiter is about twice the size of Earth.',
        "Jupiter's diameter is about eleven times Earth's diameter.",
        'Jupiter is smaller than Earth.',
      ],
      correct: "Jupiter's diameter is about eleven times Earth's diameter.",
    },
    {
      title: 'Which planets are commonly called gas giants?',
      options: ['Mercury and Venus', 'Earth and Mars', 'Jupiter and Saturn', 'Uranus and Neptune'],
      correct: 'Jupiter and Saturn',
    },
    {
      title: 'Which planets are commonly called ice giants?',
      options: ['Jupiter and Saturn', 'Uranus and Neptune', 'Earth and Mars', 'Mercury and Venus'],
      correct: 'Uranus and Neptune',
    },
    {
      title: "What does a planet's orbital period describe?",
      options: [
        'The time a planet takes to rotate once around its own axis.',
        'The time a planet takes to complete one orbit around the Sun.',
        'The time sunlight takes to reach the planet.',
        'The time between two solar eclipses.',
      ],
      correct: 'The time a planet takes to complete one orbit around the Sun.',
    },
    {
      title: 'Why are accurate scale models of the solar system difficult to show?',
      options: [
        'Because all planets are the same size.',
        'Because the distances are extremely large compared to the sizes of the planets.',
        'Because planets do not have fixed positions.',
        'Because the Sun is smaller than the planets.',
      ],
      correct: 'Because the distances are extremely large compared to the sizes of the planets.',
    },
  ];
}

function addSelfAssessment(form, title, rows, helpText) {
  form
    .addGridItem()
    .setTitle(title)
    .setHelpText(helpText)
    .setRows(rows)
    .setColumns([
      '1 - strongly disagree',
      '2',
      '3',
      '4',
      '5 - strongly agree',
    ])
    .setRequired(true);
}

function addTaskReflectionQuestions(form) {
  form
    .addSectionHeaderItem()
    .setTitle('Task Reflection')
    .setHelpText('Short follow-up questions about selected XR tasks.');

  const questions = [
    'Which fact or feature did you notice in the planet detail view?',
    'Which planets did you compare?',
    'What differences did you notice?',
    'What do you notice about the arrangement of the planets?',
    'What changed when you used the scaling controls?',
    'Did the controls help you understand the solar system better?',
  ];

  questions.forEach(function (question) {
    form.addParagraphTextItem().setTitle(question).setRequired(false);
  });
}

function addOpenQuestions(form) {
  form
    .addSectionHeaderItem()
    .setTitle('Open Questions')
    .setHelpText('Please answer briefly in your own words.');

  const questions = [
    'What helped you the most in understanding the solar system?',
    'What was unclear or difficult?',
    'Which feature or information was missing?',
    'Which scene or task was the most motivating?',
    'What should be improved before a final version?',
  ];

  questions.forEach(function (question) {
    form.addParagraphTextItem().setTitle(question).setRequired(false);
  });
}

function createObservationForm() {
  const form = FormApp.create('Solar System XR - Facilitator Observation Sheet');

  form.setDescription(
    'Observation sheet for the test facilitator. This form is not meant for participants.'
  );
  form.setCollectEmail(false);
  form.setAllowResponseEdits(true);
  form.setConfirmationMessage('Observation saved.');

  form
    .addTextItem()
    .setTitle('Participant ID')
    .setHelpText('For example: P01')
    .setRequired(true);

  const tasks = [
    'Start learning mode',
    'Read planet details',
    'Add/explore more planets',
    'Place solar system',
    'Use scaling controls',
    'Planet order minigame',
    'Planet size minigame',
  ];

  tasks.forEach(function (task) {
    form.addSectionHeaderItem().setTitle(task);

    addMultipleChoiceQuestion(form, task + ' - Successful?', [
      'Yes',
      'Partly',
      'No',
      'Not tested',
    ]);

    form
      .addTextItem()
      .setTitle(task + ' - Time')
      .setHelpText('For example: 01:30 or leave empty if not measured.')
      .setRequired(false);

    form
      .addTextItem()
      .setTitle(task + ' - Errors/Corrections')
      .setRequired(false);

    addMultipleChoiceQuestion(form, task + ' - Help', [
      '0 - no help',
      '1 - small verbal hint',
      '2 - clear help or intervention',
      '3 - task not possible without help',
    ]);

    form
      .addParagraphTextItem()
      .setTitle(task + ' - Observation/Comment')
      .setRequired(false);
  });

  form.addSectionHeaderItem().setTitle('Comfort / Motion Sickness');
  addMultipleChoiceQuestion(form, 'Comfort / motion sickness coding', [
    '0 - no signs of discomfort',
    '1 - slight discomfort',
    '2 - clear discomfort, break needed',
    '3 - test stopped due to discomfort',
  ]);

  form
    .addParagraphTextItem()
    .setTitle('Additional facilitator notes')
    .setRequired(false);

  return form;
}

function addMultipleChoiceQuestion(form, title, options) {
  const item = form.addMultipleChoiceItem().setTitle(title).setRequired(true);

  item.setChoices(
    options.map(function (option) {
      return item.createChoice(option);
    })
  );

  return item;
}
