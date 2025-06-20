// global-setup.js
const { chromium } = require('@playwright/test');
const chalk = require('chalk');

async function globalSetup(config) {
  console.log(chalk.blue.bold('🎬 Setting up TherapyDocs Demo Environment...\n'));

  // Launch browser for setup
  const browser = await chromium.launch();
  const page = await browser.newPage();

  try {
    // Wait for API to be ready
    console.log(chalk.yellow('⏳ Waiting for API server...'));
    let retries = 30;
    while (retries > 0) {
      try {
        const response = await page.request.get('http://localhost:5000/health');
        if (response.ok()) {
          console.log(chalk.green('✅ API server is ready'));
          break;
        }
      } catch (error) {
        retries--;
        if (retries === 0) {
          throw new Error('API server failed to start');
        }
        await page.waitForTimeout(2000);
      }
    }

    // Wait for database to be ready
    console.log(chalk.yellow('⏳ Waiting for database...'));
    retries = 15;
    while (retries > 0) {
      try {
        const response = await page.request.get('http://localhost:5000/health/database');
        if (response.ok()) {
          console.log(chalk.green('✅ Database is ready'));
          break;
        }
      } catch (error) {
        retries--;
        if (retries === 0) {
          console.log(chalk.yellow('⚠️  Database health check not available, continuing...'));
          break;
        }
        await page.waitForTimeout(2000);
      }
    }

    // Seed test data if needed
    console.log(chalk.yellow('🌱 Seeding test data...'));
    try {
      await page.request.post('http://localhost:5000/api/test/seed', {
        data: {
          createTestUsers: true,
          clearExisting: false
        }
      });
      console.log(chalk.green('✅ Test data seeded'));
    } catch (error) {
      console.log(chalk.yellow('⚠️  Test data seeding not available, continuing...'));
    }

    console.log(chalk.green.bold('\n✅ Demo environment setup completed!\n'));

  } catch (error) {
    console.error(chalk.red.bold('\n❌ Demo environment setup failed:'));
    console.error(chalk.red(error.message));
    throw error;
  } finally {
    await browser.close();
  }
}

module.exports = globalSetup;