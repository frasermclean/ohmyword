/**
 * Application privilege roles
 */
export enum Role {
  Admin = 'admin',
  User = 'user',
  Guest = 'guest',
}


/**
 * Part of speech of a word
 */
export enum PartOfSpeech {
  Noun = 'noun',
  Verb = 'verb',
  Adjective = 'adjective',
  Adverb = 'adverb',
}

/**
 * Reason why a round ended
 */
export enum RoundEndReason {
  Timeout = 'timeout',
  AllPlayersAwarded = 'allPlayersAwarded',
  NoPlayersLeft = 'noPlayersLeft',
}

