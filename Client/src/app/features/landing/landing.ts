import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Logo } from '../../shared/components/logo/logo';

@Component({
  selector: 'app-landing',
  imports: [RouterLink, Logo],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
})
/** Public marketing landing page — static content (stats, modules, steps, plans) + CTAs. */
export class Landing {
  stats = [
    { big: '<5 min', label: 'Full company setup' },
    { big: '50–500', label: 'Employees per tenant' },
    { big: '6+', label: 'Integrated modules' },
    { big: '100%', label: 'Tenant data isolation' },
  ];

  modules = [
    { num: '01', name: 'Identity', desc: 'Multi-tenant workspaces, JWT auth with rotating refresh tokens, and policy-based role access.' },
    { num: '02', name: 'Employees', desc: 'Profiles, lifecycle states, org chart, and instant ID cards with auth-gated QR codes.' },
    { num: '03', name: 'Assets', desc: 'Register, assignment, digital acknowledgement, and service requests with a legal audit trail.' },
    { num: '04', name: 'Attendance', desc: 'Clock in/out, leave management, weekly timesheets, and shift-aware anomaly flagging.' },
    { num: '05', name: 'Reviews', desc: 'Performance reviews, feedback cycles, goal tracking, and 360-degree assessments.' },
    { num: '06', name: 'Analytics', desc: 'Dashboards, auditor-ready PDF reports, Excel exports, and field-level audit logs.' },
  ];

  steps = [
    { n: '1', title: 'Company basics', desc: 'Name, logo, country, timezone, GST.' },
    { n: '2', title: 'Pick industry', desc: 'AI suggests your full structure instantly.' },
    { n: '3', title: 'Review', desc: 'Edit departments & asset types as cards.' },
    { n: '4', title: 'Invite HR', desc: 'First HR user gets a set-password link.' },
    { n: '5', title: 'Go live', desc: 'Dashboard loads. Import staff via Excel.' },
  ];

  plans = [
    {
      name: 'Free',
      price: '₹0',
      unit: '/mo',
      seats: 'Up to 10 employees',
      features: ['Phase 1 + 2 features', 'ID cards with QR', 'Community support'],
      cta: 'Start free',
      popular: false,
      dark: false,
    },
    {
      name: 'Starter',
      price: '₹0',
      unit: '/mo',
      seats: 'Up to 50 employees',
      features: ['All Phase 1–3', 'Email notifications', 'Excel import', 'ID card export'],
      cta: 'Choose Starter',
      popular: false,
      dark: false,
    },
    {
      name: 'Growth · Popular',
      price: '₹0',
      unit: '/mo',
      seats: 'Up to 200 employees',
      features: ['Everything in Starter', 'Full analytics suite', 'Document vault', 'Google SSO', 'API access'],
      cta: 'Choose Growth',
      popular: true,
      dark: false,
    },
    {
      name: 'Enterprise',
      price: 'Custom',
      unit: '',
      seats: '200+ employees',
      features: ['Custom branding', 'Dedicated support & SLA', 'On-prem option', 'HRMS integration'],
      cta: 'Talk to us',
      popular: false,
      dark: true,
    },
  ];
}
