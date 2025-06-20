import { test, expect } from '@playwright/test';

test.describe('Therapist Registration - Complete Demo', () => {
  
  test.beforeEach(async ({ page }) => {
    // Navigate to registration page
    await page.goto('/register');
  });

  test('AC1: Complete therapist registration flow with license validation', async ({ page }) => {
    // Record this test for demonstration
    await page.video()?.path();
    
    console.log('🎬 Starting Therapist Registration Demo...');
    
    // Fill out registration form
    await page.fill('#email', 'therapist@example.com');
    await page.fill('#password', 'SecurePass123!');
    await page.fill('#confirmPassword', 'SecurePass123!');
    await page.fill('#firstName', 'Dr. Jane');
    await page.fill('#lastName', 'Smith');
    await page.fill('#licenseNumber', 'CA-SLP-12345');
    await page.selectOption('#state', 'CA');
    await page.selectOption('#licenseType', 'speech_therapy');
    
    // Take screenshot of filled form
    await page.screenshot({ path: 'reports/screenshots/registration-form-filled.png' });
    
    // Submit registration
    await page.click('#submit-registration');
    
    // Wait for success message
    await expect(page.locator('.success-message')).toContainText('Registration successful');
    
    // Take screenshot of success
    await page.screenshot({ path: 'reports/screenshots/registration-success.png' });
    
    console.log('✅ Registration completed successfully');
  });

  test('AC2: Email verification flow', async ({ page, context }) => {
    console.log('📧 Starting Email Verification Demo...');
    
    // First register a user
    await page.fill('#email', 'verify@example.com');
    await page.fill('#password', 'SecurePass123!');
    await page.fill('#confirmPassword', 'SecurePass123!');
    await page.fill('#firstName', 'Test');
    await page.fill('#lastName', 'User');
    await page.fill('#licenseNumber', 'NY-OT-67890');
    await page.selectOption('#state', 'NY');
    await page.selectOption('#licenseType', 'occupational_therapy');
    
    await page.click('#submit-registration');
    
    // Wait for registration success
    await expect(page.locator('.success-message')).toBeVisible();
    
    // Simulate clicking email verification link
    // In real demo, this would be a real email link
    const verificationToken = 'demo-token-12345';
    await page.goto(`/verify-email?token=${verificationToken}`);
    
    // Verify account activation
    await expect(page.locator('.verification-success')).toContainText('Account verified');
    
    await page.screenshot({ path: 'reports/screenshots/email-verification-success.png' });
    
    console.log('✅ Email verification completed');
  });

  test('AC3: License validation scenarios', async ({ page }) => {
    console.log('🏥 Starting License Validation Demo...');
    
    // Test valid license
    await page.fill('#email', 'licensed@example.com');
    await page.fill('#password', 'SecurePass123!');
    await page.fill('#confirmPassword', 'SecurePass123!');
    await page.fill('#firstName', 'Licensed');
    await page.fill('#lastName', 'Therapist');
    await page.fill('#licenseNumber', 'CA-SLP-VALID');
    await page.selectOption('#state', 'CA');
    await page.selectOption('#licenseType', 'speech_therapy');
    
    await page.click('#submit-registration');
    await expect(page.locator('.success-message')).toBeVisible();
    
    // Test invalid license
    await page.goto('/register');
    await page.fill('#email', 'invalid@example.com');
    await page.fill('#password', 'SecurePass123!');
    await page.fill('#confirmPassword', 'SecurePass123!');
    await page.fill('#firstName', 'Invalid');
    await page.fill('#lastName', 'License');
    await page.fill('#licenseNumber', 'INVALID-LICENSE');
    await page.selectOption('#state', 'CA');
    await page.selectOption('#licenseType', 'speech_therapy');
    
    await page.click('#submit-registration');
    await expect(page.locator('.error-message')).toContainText('License validation failed');
    
    await page.screenshot({ path: 'reports/screenshots/license-validation-error.png' });
    
    console.log('✅ License validation scenarios completed');
  });

  test('AC4: Error handling demonstrations', async ({ page }) => {
    console.log('⚠️ Starting Error Handling Demo...');
    
    // Test weak password
    await page.fill('#email', 'weak@example.com');
    await page.fill('#password', '123');
    await page.fill('#confirmPassword', '123');
    await page.click('#submit-registration');
    
    await expect(page.locator('.password-error')).toContainText('Password too weak');
    await page.screenshot({ path: 'reports/screenshots/weak-password-error.png' });
    
    // Test duplicate email
    await page.fill('#password', 'SecurePass123!');
    await page.fill('#confirmPassword', 'SecurePass123!');
    await page.fill('#email', 'therapist@example.com'); // Already used
    await page.click('#submit-registration');
    
    await expect(page.locator('.email-error')).toContainText('Email already registered');
    await page.screenshot({ path: 'reports/screenshots/duplicate-email-error.png' });
    
    console.log('✅ Error handling demonstrations completed');
  });

  test('AC5: Timing attack prevention', async ({ page }) => {
    console.log('⏱️ Starting Timing Attack Prevention Demo...');
    
    const startTime = Date.now();
    
    // Test with non-existent email
    await page.goto('/login');
    await page.fill('#email', 'nonexistent@example.com');
    await page.fill('#password', 'anypassword');
    await page.click('#login-submit');
    
    const nonExistentTime = Date.now() - startTime;
    
    // Test with existing email but wrong password
    const startTime2 = Date.now();
    await page.fill('#email', 'therapist@example.com');
    await page.fill('#password', 'wrongpassword');
    await page.click('#login-submit');
    
    const wrongPasswordTime = Date.now() - startTime2;
    
    // Response times should be similar (within 100ms)
    const timeDifference = Math.abs(nonExistentTime - wrongPasswordTime);
    expect(timeDifference).toBeLessThan(100);
    
    console.log(`⏱️ Non-existent user: ${nonExistentTime}ms`);
    console.log(`⏱️ Wrong password: ${wrongPasswordTime}ms`);
    console.log(`⏱️ Time difference: ${timeDifference}ms`);
    console.log('✅ Timing attack prevention verified');
  });

  test('AC6: Rate limiting demonstration', async ({ page, context }) => {
    console.log('🚦 Starting Rate Limiting Demo...');
    
    // Make multiple rapid requests
    const promises = [];
    for (let i = 0; i < 10; i++) {
      promises.push(
        page.request.post('/api/auth/register', {
          data: {
            email: `test${i}@example.com`,
            password: 'SecurePass123!',
            firstName: 'Test',
            lastName: 'User',
            licenseNumber: `TEST-${i}`,
            state: 'CA',
            licenseType: 'speech_therapy'
          }
        })
      );
    }
    
    const responses = await Promise.all(promises);
    
    // Check that some requests are rate limited
    const rateLimitedResponses = responses.filter(r => r.status() === 429);
    expect(rateLimitedResponses.length).toBeGreaterThan(0);
    
    console.log(`🚦 ${rateLimitedResponses.length} requests were rate limited`);
    console.log('✅ Rate limiting demonstration completed');
  });
});