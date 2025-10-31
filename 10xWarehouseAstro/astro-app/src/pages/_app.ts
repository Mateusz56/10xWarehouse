import type { App } from 'vue'
import { pinia } from '@/stores'
import { clientRouter } from './_clientRouter'

export default (app: App) => {
  app.use(pinia)
  if (clientRouter) {
    app.use(clientRouter)
  }
  
  // Suppress router-related warnings during SSR or initial render
  const originalWarnHandler = app.config.warnHandler
  app.config.warnHandler = (msg, instance, trace) => {
    // Suppress router injection warnings - these are expected during SSR/initial render
    if (msg && typeof msg === 'string') {
      if (msg.includes('injection "Symbol(router)" not found') || 
          msg.includes('Failed to resolve component: router-view')) {
        // Suppress this warning - we handle router availability gracefully
        return
      }
    }
    // Call original handler for other warnings
    if (originalWarnHandler) {
      originalWarnHandler(msg, instance, trace)
    } else {
      console.warn(msg, instance, trace)
    }
  }
}
