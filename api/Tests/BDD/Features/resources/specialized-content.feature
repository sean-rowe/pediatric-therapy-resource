Feature: Highly Specialized Therapy Content Modules (FR-023)
  As a therapy professional
  I want access to highly specialized therapy content and protocols
  So that I can address specific conditions with evidence-based materials

  Background:
    Given I am logged in as a professional therapist
    And I have access to specialized content modules
    And content is clinically reviewed and evidence-based

  @apraxia @speech-hierarchy @not-implemented
  Scenario: Access apraxia card sets with hierarchical progression
    Given I work with students who have childhood apraxia of speech
    When I access apraxia-specific resources
    Then I should find card sets organized by:
      | Hierarchy Level      | Content Focus                  |
      | Level 1 - Vowels     | Single vowel sounds /a/, /e/, /i/|
      | Level 2 - CV patterns| Consonant-vowel combinations   |
      | Level 3 - VC patterns| Vowel-consonant combinations   |
      | Level 4 - CVC words  | Simple consonant-vowel-consonant|
      | Level 5 - Multi-syllabic| Two and three syllable words|
      | Level 6 - Sentences  | Functional phrase practice     |
    And each hierarchy should include:
      | Resource Type        | Features                       |
      | Visual cue cards     | Clear articulation photos     |
      | Audio models         | Perfect pronunciation examples |
      | Motor cues           | Tactile and gestural prompts   |
      | Progress tracking    | Mastery criteria for advancement|
      | Parent home practice | Simplified versions for home   |
      | Generalization tasks | Real-world application         |

  @minimal-pairs @phonological-awareness @not-implemented
  Scenario: Access comprehensive minimal pairs library
    Given I need to work on phonological contrasts
    When I search for minimal pairs activities
    Then I should find pairs organized by:
      | Contrast Type        | Examples                       |
      | Place contrasts      | /p/ vs /t/, /k/ vs /g/        |
      | Manner contrasts     | /p/ vs /f/, /t/ vs /s/        |
      | Voicing contrasts    | /p/ vs /b/, /t/ vs /d/        |
      | Complex contrasts    | Consonant clusters            |
    And materials should include:
      | Material Type        | Content                        |
      | Picture card pairs   | High-quality illustrated pairs |
      | Audio discrimination | Listening activities           |
      | Production practice  | Speaking exercises             |
      | Auditory bombardment | Listening exposure activities  |
      | Generalization games | Fun application activities     |
    And I should be able to:
      | Function             | Capability                     |
      | Filter by phonemes   | Find specific sound contrasts  |
      | Adjust difficulty    | Beginning to advanced levels   |
      | Track discrimination | Data on auditory skills        |
      | Track production     | Data on speaking accuracy      |
      | Generate reports     | Progress summaries             |

  @vocalic-r @all-positions @not-implemented
  Scenario: Access vocalic R resources for all word positions
    Given I specialize in R sound remediation
    When I access vocalic R materials
    Then I should find comprehensive resources for:
      | R Type               | Word Positions                 |
      | ER (stressed)        | Initial, medial, final         |
      | ER (unstressed)      | Initial, medial, final         |
      | AR                   | Initial, medial, final         |
      | OR                   | Initial, medial, final         |
      | AIR                  | Initial, medial, final         |
      | EAR                  | Initial, medial, final         |
      | IRE                  | Initial, medial, final         |
    And each R type should include:
      | Resource Category    | Materials                      |
      | Isolation practice   | R sound in isolation           |
      | Syllable practice    | R in syllables                |
      | Word level practice  | R in single words             |
      | Phrase level         | R in short phrases            |
      | Sentence level       | R in connected speech         |
      | Conversation level   | Carryover activities          |
    And specialized tools like:
      | Tool Type            | Purpose                        |
      | Visual cues          | Tongue position diagrams      |
      | Tactile cues         | Physical prompting guides     |
      | Auditory models      | Perfect R sound examples      |
      | Self-monitoring      | Student evaluation tools      |

  @feeding-therapy @oral-motor @not-implemented
  Scenario: Access comprehensive feeding therapy protocols
    Given I work with students with feeding difficulties
    When I access feeding therapy resources
    Then I should find protocols for:
      | Feeding Challenge    | Protocol Elements              |
      | Oral motor weakness  | Strengthening exercises        |
      | Sensory defensiveness| Desensitization activities     |
      | Swallowing issues    | Safe swallowing techniques     |
      | Food selectivity     | Expansion strategies           |
      | Texture progression  | Gradual texture advancement    |
      | Behavioral feeding   | Mealtime behavior management   |
    And each protocol should include:
      | Component            | Content                        |
      | Assessment tools     | Standardized feeding evaluations|
      | Intervention hierarchy| Step-by-step progression      |
      | Safety protocols     | Aspiration prevention         |
      | Family education     | Parent training materials     |
      | Progress monitoring  | Data collection sheets        |
      | Interdisciplinary    | Team collaboration guides     |

  @literacy-therapy @reading-intervention @not-implemented
  Scenario: Access literacy-based therapy units for language development
    Given I integrate literacy into speech-language therapy
    When I search for literacy-based materials
    Then I should find units targeting:
      | Literacy Skill       | Therapy Integration            |
      | Phonological awareness| Sound manipulation activities |
      | Letter knowledge     | Alphabet and sound-symbol     |
      | Decoding skills      | Reading strategy instruction  |
      | Reading comprehension| Language processing support   |
      | Narrative skills     | Story structure and retelling |
      | Vocabulary development| Word learning strategies      |
    And each unit should provide:
      | Resource Type        | Content                        |
      | Lesson plans         | Structured therapy sessions   |
      | Assessment tools     | Reading and language measures  |
      | Visual supports      | Graphic organizers and charts |
      | Technology integration| Apps and digital tools       |
      | Home extension       | Family literacy activities    |
      | Progress monitoring  | Curriculum-based measurements  |

  @social-stories @visual-supports @not-implemented
  Scenario: Access social story builder with customization options
    Given I need to create individualized social stories
    When I use the social story builder
    Then I should be able to:
      | Customization Option | Choices                        |
      | Story topic          | Pre-made or custom scenarios   |
      | Character selection  | Diverse, representative people |
      | Setting backgrounds  | School, home, community        |
      | Language level       | Adjust complexity for age     |
      | Visual style         | Photos, illustrations, symbols |
      | Story length         | Short, medium, or detailed    |
    And the builder should include:
      | Feature              | Function                       |
      | Template library     | Common social situations       |
      | Photo integration    | Upload real photos of student  |
      | Voice recording      | Add narration to stories       |
      | Multiple formats     | Print, digital, interactive    |
      | Sharing options      | Send to parents and teachers   |
      | Translation tools    | Multiple language versions     |

  @visual-schedules @structured-teaching @not-implemented
  Scenario: Create comprehensive visual schedules for various settings
    Given I support students who need visual structure
    When I access visual schedule creation tools
    Then I should be able to create schedules for:
      | Setting Type         | Schedule Elements              |
      | Daily routines       | Morning, afternoon, evening    |
      | Therapy sessions     | Activity sequence and timing   |
      | Classroom activities | Subject transitions           |
      | Social situations    | Step-by-step social scripts    |
      | Life skills tasks    | Task analysis breakdowns      |
      | Behavioral supports  | Coping strategy sequences     |
    And customization should include:
      | Visual Element       | Options                        |
      | Symbol system        | PECS, Boardmaker, photos      |
      | Layout style         | Horizontal, vertical, grid    |
      | Timing indicators    | Clocks, timers, countdowns    |
      | Completion tracking  | Checkboxes, stars, stamps     |
      | Transition cues      | First/then, next, finished    |
      | Portable formats     | Laminated, digital, wearable  |

  @articulation-hierarchies @motor-planning @not-implemented
  Scenario: Access detailed articulation treatment hierarchies
    Given I provide articulation therapy for various sound errors
    When I select a target phoneme
    Then I should find treatment hierarchies including:
      | Hierarchy Level      | Activities                     |
      | Auditory discrimination| Can hear the difference       |
      | Sound isolation      | Produce sound alone           |
      | Syllable level       | Sound in CV, VC, CVC          |
      | Word level initial   | Sound at beginning of words   |
      | Word level medial    | Sound in middle of words      |
      | Word level final     | Sound at end of words         |
      | Phrase level         | Sound in short phrases        |
      | Sentence level       | Sound in sentences            |
      | Conversation level   | Sound in spontaneous speech   |
      | Generalization       | Sound in all environments     |
    And each level should provide:
      | Support Material     | Content                        |
      | Stimulus materials   | Words and pictures for practice|
      | Cuing strategies     | Visual, auditory, tactile     |
      | Data collection      | Progress tracking sheets      |
      | Homework materials   | Home practice activities      |
      | Mastery criteria     | When to advance levels        |

  @complex-communication @teen-resources @not-implemented
  Scenario: Access article companions and activities for teenagers
    Given I work with teenagers on advanced language skills
    When I access teen-focused materials
    Then I should find:
      | Resource Type        | Content Focus                  |
      | Current events articles| Age-appropriate news stories |
      | Academic text supports| Textbook comprehension aids   |
      | Social media analysis| Digital communication skills  |
      | Career exploration   | Job interview and resume skills|
      | Abstract reasoning   | Higher-level thinking tasks    |
      | Peer interaction     | Social communication scenarios |
    And activities should target:
      | Language Skill       | Intervention Approach          |
      | Reading comprehension| Graphic organizers and strategies|
      | Written expression   | Essay structure and editing   |
      | Oral presentation    | Public speaking and confidence |
      | Critical thinking    | Analysis and evaluation tasks  |
      | Vocabulary expansion | Academic and technical terms   |
      | Pragmatic skills     | Real-world communication      |

  @protocol-fidelity @evidence-tracking @not-implemented
  Scenario: Ensure protocol fidelity with evidence-based implementation
    Given I want to implement specialized protocols correctly
    When I access any specialized content module
    Then I should find:
      | Fidelity Support     | Content                        |
      | Implementation guides| Step-by-step procedures       |
      | Training materials   | How to learn the protocol     |
      | Fidelity checklists  | Self-monitoring tools         |
      | Video demonstrations | Expert implementation examples |
      | Troubleshooting guides| Common problems and solutions|
      | Outcome measures     | How to track effectiveness    |
    And evidence base should include:
      | Evidence Type        | Information                    |
      | Research citations   | Supporting scientific studies  |
      | Efficacy data        | Treatment effectiveness rates  |
      | Best practice guidelines| Professional recommendations |
      | Contraindications    | When not to use the protocol  |
      | Modification guides  | Adapting for different populations|