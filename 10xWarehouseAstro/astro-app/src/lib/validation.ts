// Validation utilities for forms

export interface ValidationResult {
  isValid: boolean;
  message?: string;
}

export function validateDisplayName(displayName: string): ValidationResult {
  if (!displayName || displayName.trim().length === 0) {
    return { isValid: false, message: 'Display name is required' };
  }
  
  if (displayName.length < 2) {
    return { isValid: false, message: 'Display name must be at least 2 characters long' };
  }
  
  if (displayName.length > 100) {
    return { isValid: false, message: 'Display name must be no more than 100 characters long' };
  }
  
  // Check for valid characters (letters, numbers, spaces, hyphens, underscores)
  if (!/^[a-zA-Z0-9\s\-_]+$/.test(displayName)) {
    return { isValid: false, message: 'Display name can only contain letters, numbers, spaces, hyphens, and underscores' };
  }
  
  // Check for consecutive spaces
  if (/\s{2,}/.test(displayName)) {
    return { isValid: false, message: 'Display name cannot contain consecutive spaces' };
  }
  
  // Check for leading/trailing spaces
  if (displayName !== displayName.trim()) {
    return { isValid: false, message: 'Display name cannot start or end with spaces' };
  }
  
  return { isValid: true };
}

export function validatePassword(password: string): ValidationResult {
  if (!password || password.length === 0) {
    return { isValid: false, message: 'Password is required' };
  }
  
  if (password.length < 6) {
    return { isValid: false, message: 'Password must be at least 6 characters long' };
  }
  
  if (password.length > 100) {
    return { isValid: false, message: 'Password must be no more than 100 characters long' };
  }
  
  return { isValid: true };
}

export function validatePasswordConfirmation(password: string, confirmPassword: string): ValidationResult {
  if (!confirmPassword || confirmPassword.length === 0) {
    return { isValid: false, message: 'Password confirmation is required' };
  }
  
  if (password !== confirmPassword) {
    return { isValid: false, message: 'Passwords do not match' };
  }
  
  return { isValid: true };
}

export function calculatePasswordStrength(password: string): {
  score: number;
  label: string;
  color: string;
  feedback: string[];
} {
  let score = 0;
  const feedback: string[] = [];
  
  // Length checks
  if (password.length >= 6) score++;
  else feedback.push('At least 6 characters');
  
  if (password.length >= 8) score++;
  else if (password.length >= 6) feedback.push('8+ characters for better security');
  
  // Character variety checks
  if (/[a-z]/.test(password)) score++;
  else feedback.push('Lowercase letters');
  
  if (/[A-Z]/.test(password)) score++;
  else feedback.push('Uppercase letters');
  
  if (/[0-9]/.test(password)) score++;
  else feedback.push('Numbers');
  
  if (/[^A-Za-z0-9]/.test(password)) score++;
  else feedback.push('Special characters');
  
  // Common password checks
  const commonPasswords = ['password', '123456', 'qwerty', 'abc123', 'password123'];
  if (commonPasswords.some(common => password.toLowerCase().includes(common))) {
    score = Math.max(0, score - 2);
    feedback.push('Avoid common passwords');
  }
  
  // Sequential character checks
  if (/(.)\1{2,}/.test(password)) {
    score = Math.max(0, score - 1);
    feedback.push('Avoid repeated characters');
  }
  
  const labels = ['Very Weak', 'Weak', 'Fair', 'Good', 'Strong'];
  const colors = ['red', 'orange', 'yellow', 'blue', 'green'];
  
  const finalScore = Math.min(score, 4);
  
  return {
    score: finalScore,
    label: labels[finalScore],
    color: colors[finalScore],
    feedback: feedback.slice(0, 3) // Limit to 3 feedback items
  };
}

export function validateEmail(email: string): ValidationResult {
  if (!email || email.trim().length === 0) {
    return { isValid: false, message: 'Email is required' };
  }
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(email)) {
    return { isValid: false, message: 'Please enter a valid email address' };
  }
  
  return { isValid: true };
}
