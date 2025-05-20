resource "aws_ssm_parameter" "allowed_admin_groups" {
  name  = "/ta-housing/pre-production/asset-api-admin-allowed-groups"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "disallowed_email" {
  name  = "/housing-tl/pre-production/disallowed-email"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}

resource "aws_ssm_parameter" "edit_patches_admin_group" {
  name  = "/ta-housing/pre-production/asset-api-edit-patches-admin-allowed-groups"
  type  = "String"
  value = "to_be_set_manually"

  lifecycle {
    ignore_changes = [
      value,
    ]
  }
}
