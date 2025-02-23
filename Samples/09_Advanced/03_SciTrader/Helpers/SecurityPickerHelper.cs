using StockSharp.BusinessEntities;
using StockSharp.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SciTrader.Helpers
{
	public static class SecurityPickerHelper
	{
		public static readonly DependencyProperty BoundSecurityProperty =
			DependencyProperty.RegisterAttached(
				"BoundSecurity",
				typeof(Security),
				typeof(SecurityPickerHelper),
				new PropertyMetadata(null, OnBoundSecurityChanged));

		public static void SetBoundSecurity(DependencyObject element, Security value)
		{
			element.SetValue(BoundSecurityProperty, value);
		}

		public static Security GetBoundSecurity(DependencyObject element)
		{
			return (Security)element.GetValue(BoundSecurityProperty);
		}

		private static void OnBoundSecurityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SecurityPicker picker)
			{
				picker.SelectedSecurity = (Security)e.NewValue;
			}
		}

		public static readonly DependencyProperty EnableBindingProperty =
			DependencyProperty.RegisterAttached(
				"EnableBinding",
				typeof(bool),
				typeof(SecurityPickerHelper),
				new PropertyMetadata(false, OnEnableBindingChanged));

		public static void SetEnableBinding(DependencyObject element, bool value)
		{
			element.SetValue(EnableBindingProperty, value);
		}

		public static bool GetEnableBinding(DependencyObject element)
		{
			return (bool)element.GetValue(EnableBindingProperty);
		}

		private static void OnEnableBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is SecurityPicker picker && (bool)e.NewValue)
			{
				picker.SecuritySelected += (security) =>
				{
					SetBoundSecurity(picker, security);
				};
			}
		}
	}
}
