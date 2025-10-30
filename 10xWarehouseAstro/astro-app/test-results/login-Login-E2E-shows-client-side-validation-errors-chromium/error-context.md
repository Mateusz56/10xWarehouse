# Page snapshot

```yaml
- generic [ref=e1]:
  - main [ref=e4]:
    - generic [ref=e7]:
      - heading "Sign in to your account" [level=2] [ref=e9]
      - generic [ref=e12]:
        - generic [ref=e13]:
          - generic [ref=e14]: Email
          - textbox "Email" [active] [ref=e15]:
            - /placeholder: Enter your email
        - generic [ref=e16]:
          - generic [ref=e17]: Password
          - textbox "Password" [ref=e18]:
            - /placeholder: Enter your password
        - button "Sign In" [ref=e19]
        - generic [ref=e20]:
          - text: Don't have an account?
          - link "Sign up" [ref=e21] [cursor=pointer]:
            - /url: /register
  - generic [ref=e24]:
    - button "Menu" [ref=e25]:
      - img [ref=e27]
      - generic: Menu
    - button "Inspect" [ref=e31]:
      - img [ref=e33]
      - generic: Inspect
    - button "Audit" [ref=e35]:
      - img [ref=e37]
      - generic: Audit
    - button "Settings" [ref=e40]:
      - img [ref=e42]
      - generic: Settings
```