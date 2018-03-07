'use strict';
import i18n from 'i18next';
import XHR from 'i18next-xhr-backend';
import LanguageDetector from 'i18next-browser-languagedetector';

const options = {
  fallbackLng: 'en',
  load: 'languageOnly',
  ns: ['common'],
  defaultNS: 'common',
  debug: true,
  interpolation: {
    escapeValue: false, // not needed for react!!
    formatSeparator: ',',
    format: (value, format, lng) => {
      if (format === 'uppercase') return value.toUpperCase();
      return value;
    }
  }
};

if (process.browser) {
  i18n
    .use(XHR)
    .use(LanguageDetector);
}

if (!i18n.isInitialized) i18n.init(options);
export default i18n;